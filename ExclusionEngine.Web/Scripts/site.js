function showCassModal(entered, standardized) {
  document.getElementById('enteredAddress').innerText = entered;
  document.getElementById('cassAddress').innerText = standardized;
  document.getElementById('confirmModal').classList.remove('hidden');
}

function keepOriginalAndSave() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('MainContent_ConfirmedStandardized').value = 'true';
  document.getElementById('MainContent_UseOriginalAddress').value = 'true';
  document.getElementById('MainContent_ValidateAddressButton').click();
}

function acceptCassChanges() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('MainContent_ConfirmedStandardized').value = 'true';
  document.getElementById('MainContent_UseOriginalAddress').value = 'false';
  document.getElementById('MainContent_ValidateAddressButton').click();
}
