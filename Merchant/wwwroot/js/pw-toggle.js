document.querySelectorAll('.pw_toggle_btn').forEach(function(btn) {
    btn.addEventListener('click', function() {
        var input = document.getElementById(btn.dataset.target);
        var icon = btn.querySelector('i');
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.replace('fa-eye', 'fa-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.replace('fa-eye-slash', 'fa-eye');
        }
    });
});