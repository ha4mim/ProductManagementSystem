$(document).ready(function () {
    $('form').submit(function (e) {
        var photoInput = $('#PhotoUpload')[0].files[0];
        if (photoInput && photoInput.size > 5 * 1024 * 1024) {
            alert("Photo must be less than 5MB.");
            e.preventDefault();
        }
    });
});
