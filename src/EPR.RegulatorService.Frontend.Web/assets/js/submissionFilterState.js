'use strict';

document.addEventListener("DOMContentLoaded", () => {
    const submissionYearsWrapElement = document.querySelector('.submission-years');
    const submissionYearElements = document.querySelectorAll('.submission-years input');
    const submissionPeriodElements = document.querySelectorAll('.submission-periods input');

    const updateFilterState = () => {
        const yearsState = {};
        let allYearsUnchecked = true;

        for (const yearElement of submissionYearElements) {
            yearsState[yearElement.value] = yearElement.checked;

            allYearsUnchecked = allYearsUnchecked && !yearElement.checked;
        }

        if (allYearsUnchecked) {
            for (const periodElement of submissionPeriodElements) {
                periodElement.disabled = false;
            }
        } else {
            for (const periodElement of submissionPeriodElements) {
                const year = periodElement.value.slice(-4);

                periodElement.disabled = !yearsState[year];
            }
        }
    };

    updateFilterState();

    if (submissionYearsWrapElement !== null) {
        submissionYearsWrapElement.addEventListener('click', (event) => {
            if (event.target.nodeName !== 'INPUT') {
                return;
            }

            updateFilterState();
        });
    }
});
