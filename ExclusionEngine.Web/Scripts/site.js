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

    el.innerText = parsed.toLocaleDateString();
    el.title = 'UTC: ' + utcValue;
  }
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', function() {
    localizeCreatedAtTimes();
    initPhoneFormatting();
  });
} else {
  localizeCreatedAtTimes();
  initPhoneFormatting();
}


function formatPhoneValue(rawValue) {
  var value = rawValue || '';
  var extMatch = value.match(/^(.*?)(?:\s*(?:ext\.?|x)\s*([a-z0-9-]*))$/i);
  var basePart = value;
  var extension = '';

  if (extMatch) {
    basePart = extMatch[1];
    extension = (extMatch[2] || '').replace(/[^a-z0-9-]/gi, '');
  }

  var digits = basePart.replace(/\D/g, '');
  var formatted = '';

  if (digits.length <= 3) {
    formatted = digits;
  } else if (digits.length <= 6) {
    formatted = digits.slice(0, 3) + '-' + digits.slice(3);
  } else {
    formatted = digits.slice(0, 3) + '-' + digits.slice(3, 6) + '-' + digits.slice(6, 10);
    if (digits.length > 10) {
      formatted += ' ' + digits.slice(10);
    }
  }

  if (extension) {
    formatted += ' ext ' + extension;
  }

  return formatted.trim();
}

function initPhoneFormatting() {
  var phoneInputs = document.querySelectorAll('input.phone-format');
  for (var i = 0; i < phoneInputs.length; i++) {
    var input = phoneInputs[i];

    input.value = formatPhoneValue(input.value);
    input.addEventListener('input', function(e) {
      e.target.value = formatPhoneValue(e.target.value);
    });
    input.addEventListener('blur', function(e) {
      e.target.value = formatPhoneValue(e.target.value);
    });
  }
}

function toggleSelectAll(source) {
  source.closest('table').querySelectorAll('tbody input[type=checkbox]')
    .forEach(function(cb) { cb.checked = source.checked; });
}

function confirmBulkDelete(gridClass) {
  var count = document.querySelectorAll(gridClass + ' tbody input[type=checkbox]:checked').length;
  if (count === 0) {
    alert('Please select at least one entry to delete.');
    return false;
  }
  return confirm('Delete ' + count + ' selected entr' + (count === 1 ? 'y' : 'ies') + '?');
}
