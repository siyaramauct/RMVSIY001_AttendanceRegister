// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function updateAttendanceAction(statusSelect) {
	const form = statusSelect.closest('form');
	const updateButton = form?.querySelector('.update-btn');

	if (!updateButton) {
		return;
	}

	updateButton.hidden = statusSelect.value === statusSelect.dataset.originalStatus;
}
