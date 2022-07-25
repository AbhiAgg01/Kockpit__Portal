// Toggle Password Visibility

const togglePasswordVisibility = document.querySelectorAll(".toggle-password-visible");

togglePasswordVisibility.forEach(item => {
    item.addEventListener("click", () => {
        item.parentElement.firstElementChild.focus();
        if(item.classList.contains("active")) {
            item.classList.remove("active");
            item.parentElement.firstElementChild.type = 'password';
        } else {
            item.classList.add("active");
            item.parentElement.firstElementChild.type = 'text';
        }
    });
});