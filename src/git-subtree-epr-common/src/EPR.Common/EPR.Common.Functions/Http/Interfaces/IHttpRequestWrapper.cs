namespace EPR.Common.Functions.Http.Interfaces
{
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.Mvc;

    public interface IHttpRequestWrapper<TPermission>
    {
        public Task<ActionResult> Execute(List<TPermission> requiredPermissions, Func<Task<ActionResult>> implementation, CancellationToken cancellationToken, [CallerMemberName] string functionName = null);

        Task<ActionResult<TResult>> Execute<TResult>(List<TPermission> requiredPermissions, Func<Task<ActionResult<TResult>>> implementation, CancellationToken cancellationToken, [CallerMemberName] string functionName = null);
    }
}