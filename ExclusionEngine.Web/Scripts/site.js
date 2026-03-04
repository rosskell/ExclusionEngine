function showCassModal(entered, standardized) {
  document.getElementById('enteredAddress').innerText = entered;
  document.getElementById('cassAddress').innerText = standardized;
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
