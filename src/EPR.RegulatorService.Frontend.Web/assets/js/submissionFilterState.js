'use strict';

document.addEventListener("DOMContentLoaded", () => {
  const submissionYearsWrapElement = document.querySelector('.submission-years');
  const submissionYearElements = document.querySelectorAll('.submission-years input');
  const submissionPeriodElements = document.querySelectorAll('.submission-periods input');

  const getYearsState = () => {
    const state = {};
    let allUnchecked = true;

    submissionYearElements.forEach(yearElement => {
      const checked = yearElement.checked;
      state[yearElement.value] = checked;
      if (checked) allUnchecked = false;
    });

    return { state, allUnchecked };
  };

  const setPeriodElementsState = (yearsState, allUnchecked) => {
    submissionPeriodElements.forEach(periodElement => {
      if (allUnchecked) {
        periodElement.disabled = false;
      } else {
        const year = periodElement.value.slice(-4);
        periodElement.disabled = !yearsState[year];
      }
    });
  };

  const updateFilterState = () => {
    const { state, allUnchecked } = getYearsState();
    setPeriodElementsState(state, allUnchecked);
  };

  updateFilterState();

  if (submissionYearsWrapElement) {
    submissionYearsWrapElement.addEventListener('click', (event) => {
      if (event.target.nodeName === 'INPUT') {
        updateFilterState();
      }
    });
  }
});
