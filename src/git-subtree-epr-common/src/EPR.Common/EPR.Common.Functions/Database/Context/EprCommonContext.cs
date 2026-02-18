namespace EPR.Common.Functions.Database.Context;

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using AccessControl.Interfaces;
using Converters;
using Decorators.Interfaces;
using Entities;
using Entities.Interfaces;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

public abstract class EprCommonContext : DbContext, IEprCommonContext
{
    private readonly IRequestTimeService requestTimeService;
    private readonly IEnumerable<IEntityDecorator> entityDecorators;
    private readonly IUserContextProvider userContextProvider;

    private readonly Dictionary<Type, PropertyInfo> entitySetProperties;
    private readonly Dictionary<Type, PropertyInfo> auditEntitySetProperties;

    public EprCommonContext(
        DbContextOptions contextOptions,
        IUserContextProvider userContextProvider,
        IRequestTimeService requestTimeService,
        IEnumerable<IEntityDecorator> entityDecorators)
        : base(contextOptions)
    {
        this.requestTimeService = requestTimeService;
        this.entityDecorators = entityDecorators;
        this.userContextProvider = userContextProvider;

        this.entitySetProperties = this.GetType().GetProperties()
            .Where(propertyInfo => propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) && !UncheckedEntities.Contains(propertyInfo.PropertyType.GetGenericArguments()[0]))
            .ToDictionary(
                x => x.PropertyType.GetGenericArguments()[0],
                x => x);

        this.auditEntitySetProperties = this.entitySetProperties
            .Where(kvp => kvp.Key.GetInterfaces().Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAudit<>))).ToDictionary(
                kvp => kvp.Key.GetInterfaces().Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAudit<>)),
                kvp => kvp.Value);

        this.AuditTypeMap = this.auditEntitySetProperties.ToDictionary(
            kvp => kvp.Key.GetGenericArguments()[0],
            kvp => kvp.Value.PropertyType.GetGenericArguments()[0]);
    }

    public static List<Type> UncheckedEntities { get; } = new ()
    {
        typeof(Migration),
    };

    public Dictionary<Type, Type> AuditTypeMap { get; set; }

    public override int SaveChanges()
    {
        this.SaveAudit(this.GetChanges());
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.SaveAudit(this.GetChanges());
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var addQueryFilterMethodInfo = DelegateReflection.GenericMethodOf<Action<ModelBuilder>>(x => this.AddQueryFilter<EntityBase>(x));

        foreach (var (entityType, entitySetProperty) in this.entitySetProperties)
        {
            if (!typeof(EntityBase).IsAssignableFrom(entityType))
            {
                throw new MissingMethodException($"Entity type {entityType.Name} of entity set {entitySetProperty.Name} should derive from {nameof(EntityBase)}.");
            }

            addQueryFilterMethodInfo.MakeGenericMethod(entityType).Invoke(this, new[] { modelBuilder });

            var auditedEntityType = entityType.GetInterfaces().SingleOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAudit<>))?.GetGenericArguments()[0];

            if (auditedEntityType == null && !this.auditEntitySetProperties.ContainsKey(typeof(IAudit<>).MakeGenericType(entityType)))
            {
                throw new MissingMethodException($"Entity set {entitySetProperty.Name} has no corresponding audit entity set implementing IAudit<{entityType.Name}>.");
            }

            if (auditedEntityType != null)
            {
                if (!this.entitySetProperties.ContainsKey(auditedEntityType))
                {
                    throw new MissingMethodException($"Audit entity set {entitySetProperty.Name} has no corresponding entity set of type {auditedEntityType.Name}.");
                }

                var entityProperties = auditedEntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var auditEntityProperties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var auditProperties = typeof(IAudit).GetProperties();

                foreach (var entityProperty in entityProperties)
                {
                    if (this.entitySetProperties.ContainsKey(entityProperty.PropertyType) ||
                        entityProperty.CustomAttributes.Any(attr => attr.AttributeType == typeof(NotMappedAttribute)))
                    {
                        continue;
                    }

                    var auditPropertyName = $"_{entityProperty.Name}";
                    if (!auditEntityProperties.Any(auditProperty => auditProperty.Name == auditPropertyName && auditProperty.PropertyType == entityProperty.PropertyType))
                    {
                        throw new MissingMethodException($"Audit entity type {entityType.Name} should implement {auditPropertyName}.");
                    }
                }

                foreach (var auditProperty in auditEntityProperties)
                {
                    if (auditProperties.Any(entityProperty => entityProperty.Name == auditProperty.Name))
                    {
                        continue;
                    }

                    var entityPropertyName = auditProperty.Name.Substring(1);
                    if (entityPropertyName != "Id" && !entityProperties.Any(entityProperty => entityProperty.Name == entityPropertyName && entityProperty.PropertyType == auditProperty.PropertyType))
                    {
                        throw new MissingMethodException($"Audit entity type {entityType.Name} should not implement {auditProperty.Name}.");
                    }
                }
            }
        }

        // EF should not track or attempt to create this entity/table
        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasNoKey();

            entity.ToTable("__EFMigrationsHistory", t => t.ExcludeFromMigrations());
        });

        this.ConfigureKeys(modelBuilder);
        modelBuilder.ApplyUtcDateTimeConverter();
    }

    protected abstract void ConfigureApplicationKeys(ModelBuilder modelBuilder);

    private static void ConfigureEntityKeys<T>(ModelBuilder modelBuilder)
        where T : EntityBase =>
        modelBuilder.Entity<T>().HasIndex(x => new { x.Id }).IsUnique();

    private static void Truncate<T>(DbSet<T> entitySet)
        where T : EntityBase =>
        entitySet.RemoveRange(entitySet);

    private Dictionary<EntityState, List<object>> GetChanges()
    {
        this.ChangeTracker.DetectChanges();
        var changes = this.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged && entry.Entity is EntityBase).ToList();
        foreach (var entityDecorator in this.entityDecorators)
        {
            entityDecorator.BatchStart();

            foreach (var entry in changes)
            {
                entityDecorator.Decorate(entry);
            }

            entityDecorator.BatchComplete();
        }

        return changes.GroupBy(x => x.State).ToDictionary(
            group => group.Key,
            group => group.Select(entry => entry.Entity).ToList());
    }

    private void AddQueryFilter<T>(ModelBuilder modelBuilder)
        where T : EntityBase
    {
        // Intentionally left empty
    }

    private void SaveAudit(Dictionary<EntityState, List<object>> changes)
    {
        var saveAuditMethodInfo = DelegateReflection
            .MethodOf<Action<EntityState, EntityBase>>((entityState, entity) => this.SaveAudit<MockEntity, MockAuditEntity>(default, default))
            .GetGenericMethodDefinition();

        foreach (var (entityState, entities) in changes)
        {
            foreach (var entity in entities)
            {
                var entityType = entity.GetType();
                if (!typeof(AuditEntityBase).IsAssignableFrom(entityType))
                {
                    saveAuditMethodInfo.MakeGenericMethod(entityType, this.AuditTypeMap[entityType]).Invoke(this, new[] { entityState, entity });
                }
            }
        }
    }

    private void SaveAudit<T, TAudit>(EntityState entityState, T entity)
        where T : EntityBase
        where TAudit : AuditEntityBase, new()
    {
        var auditInstance = new TAudit();

        var entityProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (var entityProperty in entityProperties)
        {
            if (this.entitySetProperties.ContainsKey(entityProperty.PropertyType) ||
                entityProperty.CustomAttributes.Any(attr => attr.AttributeType == typeof(NotMappedAttribute)))
            {
                continue;
            }

            var value = typeof(T).GetProperty(entityProperty.Name)?.GetValue(entity);
            typeof(TAudit).GetProperty("_" + entityProperty.Name)?.SetValue(auditInstance, value);
        }

        auditInstance._Id = entity.Id;
        auditInstance.EntityState = (int)entityState;
        auditInstance.UserId = this.userContextProvider.UserId;
        auditInstance.EmailAddress = this.userContextProvider.EmailAddress;
        auditInstance.AuditCreated = this.requestTimeService.UtcRequest;
        this.Add(auditInstance);
    }

    private void ConfigureKeys(ModelBuilder modelBuilder)
    {
        var configureEntityKeysMethodInfo = DelegateReflection.GenericMethodOf<Action<ModelBuilder>>(x => ConfigureEntityKeys<EntityBase>(x));

        var entitySetProps = this.entitySetProperties.Keys;

        foreach (var entityType in entitySetProps)
        {
            configureEntityKeysMethodInfo.MakeGenericMethod(entityType).Invoke(this, new object[] { modelBuilder });
        }

        this.ConfigureApplicationKeys(modelBuilder);
    }
}