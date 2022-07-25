// Expand Menu
const menuToggle = document.getElementById("menu-toggle");
const menu = document.getElementById("menu");
const offerings = document.getElementById("offerings");
const hamBtn = document.getElementById("ham-btn");
const closeBtn = document.getElementById("close-menu");


menuToggle.addEventListener("click", () => {
    if (window.innerWidth > 768) {
        menu.classList.toggle("expanded");
        offerings.classList.toggle("expanded");
        hamBtn.classList.toggle("active");
    } else {
        menu.classList.add("mobile-expanded");
    }
});

closeBtn.addEventListener("click", () => {
    menu.classList.remove("mobile-expanded");
});


// Card hover play pause
const cardVideo = Array.from(document.querySelectorAll(".card-video"));
const videoContainer = Array.from(document.querySelectorAll(".video-container"));

for (let i = 0; i < videoContainer.length; ++i) {
    videoContainer[i].addEventListener("mouseenter", () => {
        cardVideo[i].play();
    });

    videoContainer[i].addEventListener("mouseleave", () => {
        cardVideo[i].pause();
    })
}

// Toggle submenu in menu
const outerItems = Array.from(document.querySelectorAll(".outer-item"));

outerItems.forEach((item, index) => {
    item.addEventListener("click", () => {
        if (item.classList.contains("active")) {
            item.classList.remove("active");
        } else {
            outerItems.forEach(item => {
                item.classList.remove("active");
            });

            outerItems[index].classList.toggle("active");
        }
    });
});

const submenuItem = Array.from(document.querySelectorAll(".submenu .menu-nav-item"));

submenuItem.forEach(item => {
    item.addEventListener("click", () => {
        submenuItem.forEach(item => {
            item.classList.remove("active");
        });

        item.classList.add("active");
    });
});