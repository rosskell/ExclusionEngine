function showCassModal(entered, standardized, hasError, errorMessage) {
  document.getElementById('enteredAddress').innerText = entered;
  document.getElementById('cassAddress').innerText = standardized;

  var statusEl = document.getElementById('cassStatus');
  if (hasError) {
    statusEl.innerText = 'CASS issue: ' + (errorMessage || 'Address could not be fully validated.');
    statusEl.className = 'error';
  } else {
    statusEl.innerText = 'CASS returned a standardized result. Choose how to save.';
    statusEl.className = 'warn';
  }

  document.getElementById('confirmModal').classList.remove('hidden');
}

function keepOriginalAndSave() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('ConfirmedStandardized').value = 'true';
  document.getElementById('UseOriginalAddress').value = 'true';
  document.getElementById('ValidateAddressButton').click();
}

function acceptCassChanges() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('ConfirmedStandardized').value = 'true';
  document.getElementById('UseOriginalAddress').value = 'false';
  document.getElementById('ValidateAddressButton').click();
}

function cancelCassPrompt() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('ConfirmedStandardized').value = 'false';
  document.getElementById('UseOriginalAddress').value = 'false';
}

function localizeCreatedAtTimes() {
  var nodes = document.querySelectorAll('[data-utc-created]');
  for (var i = 0; i < nodes.length; i++) {
    var el = nodes[i];
    var utcValue = el.getAttribute('data-utc-created');
    if (!utcValue) {
      continue;
    }

    var parsed = new Date(utcValue);
    if (isNaN(parsed.getTime())) {
      continue;
    }

    el.innerText = parsed.toLocaleString();
    el.title = 'UTC: ' + utcValue;
  }
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', localizeCreatedAtTimes);
} else {
  localizeCreatedAtTimes();
}
