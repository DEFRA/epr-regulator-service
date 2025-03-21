function InitSpinner(contentWrapperId, buttonId) {
  const spinner = document.getElementById('global-spinner');
  const button = document.getElementById(buttonId);
  const contentWrapper = document.getElementById(contentWrapperId);
  let spinnerTimeout;
  if (spinner) {
    spinner.style.display = 'none';
  }
  if (contentWrapper) {
    if (button) {
      button.addEventListener("click", function (event) {
        clearTimeout(spinnerTimeout);
        showSpinnerAndHideContentWithDelay();
      });
    }
  }
  function showSpinnerAndHideContentWithDelay() {
    spinnerTimeout = setTimeout(() => {
      if (spinner) {
        spinner.style.display = 'flex';
      }
      if (contentWrapper) {
        contentWrapper.style.display = 'none';
      }
    }, 500);
  }
}
