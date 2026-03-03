function showCassModal(entered, standardized) {
  document.getElementById('enteredAddress').innerText = entered;
  document.getElementById('cassAddress').innerText = standardized;
  document.getElementById('confirmModal').classList.remove('hidden');
}

function closeCassModal() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('confirmModal').style.display = 'none';
  __doPostBack('', '');
}

function acceptCassChanges() {
  document.getElementById('confirmModal').classList.add('hidden');
  document.getElementById('MainContent_ConfirmedStandardized').value = 'true';
  document.getElementById('MainContent_ValidateAddressButton').click();
}
