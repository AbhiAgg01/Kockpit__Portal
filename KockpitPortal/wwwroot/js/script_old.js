// Expand Menu
const menuToggle = document.getElementById("menu-toggle");
const menu = document.getElementById("menu");
const offerings = document.getElementById("offerings");
const hamBtn = document.getElementById("ham-btn");

menuToggle.addEventListener("click", () => {
    menu.classList.toggle("expanded");
    offerings.classList.toggle("expanded");
    hamBtn.classList.toggle("active");
});


// Card hover play pause
const cardVideo = Array.from(document.querySelectorAll("#card-video"));
const videoContainer = Array.from(document.querySelectorAll("#video-container"));

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

        outerItems[index].classList.toggle("active");

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