const fullScreenToggleBtn = document.querySelector(".full-screen-toggle");

function toggleFullScreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        }
    }
}

fullScreenToggleBtn.addEventListener("click", () => {
    toggleFullScreen();
});

const dropdownLinks = document.querySelectorAll(".menu-item");

dropdownLinks.forEach(dropdown => {
    dropdown.addEventListener("click", () => {
        console.log("clicked");
        if (dropdown.classList.contains("dropdown-expanded")) {
            dropdown.classList.remove("dropdown-expanded");
        } else {
            dropdownLinks.forEach(item => {
                item.classList.remove("dropdown-expanded");
            })
            dropdown.classList.toggle("dropdown-expanded");
        }
    });
});


const menuLinks = document.querySelectorAll("menu-item-link");

menuLinks.forEach(link => {
    link.addEventListener("click", () => {
        menuLinks.forEach(item => {
            item.classList.remove("active");
        });
        link.classList.add("active");
    });
});
// menuLinks.forEach(link => {
//   link.addEventListener("click", (e) => {
//     menuLinks.forEach(item => {
//       item.classList.remove("active");
//     });
//     link.classList.add("active");

//     if(link.classList.contains("has-dropdown")) {
//       console.log("here");
//       if(link.classList.contains("dropdown-expanded")) {
//         link.classList.remove("dropdown-expanded");
//       } else {
//         menuLinks.forEach(item => {
//           item.classList.remove("dropdown-expanded");
//         })
//         link.classList.add("dropdown-expanded");
//       }
//     }
//   })
// });

// menu logic

const menuToggleBtn = document.querySelector(".menu-toggle");
const sidebarCloseBtn = document.querySelector(".sidebar-close-btn");
const sidebar = document.querySelector(".sidebar");
const main = document.querySelector(".main");
const header = document.querySelector(".header");

menuToggleBtn.addEventListener("click", () => {
    if (window.innerWidth > 1000) {
        if (!sidebar.classList.contains("shrinked")) {
            sidebar.classList.add("shrinked");
            header.classList.add("expanded");
            main.classList.add("expanded");
        } else {
            sidebar.classList.remove("shrinked");
            header.classList.remove("expanded");
            main.classList.remove("expanded");
        }
    } else {
        sidebar.classList.add("mobile-show");
    }
});

sidebarCloseBtn.addEventListener("click", () => {
    sidebar.classList.remove("mobile-show");
});

// header dropdown

const userBtn = document.querySelector(".user .header-btn");
const headerDropdown = document.querySelector(".header-dropdown");

userBtn.addEventListener("click", (e) => {
    e.stopPropagation();
    headerDropdown.classList.toggle("shown");
});

window.addEventListener("click", () => {
    if (headerDropdown.classList.contains("shown")) {
        headerDropdown.classList.remove("shown");
    }
});

// cart dropdown
//const cartBtn = document.querySelector(".header-btn.cart-btn");
//const cartDropdown = document.querySelector(".cart-dropdown");
//cartBtn.addEventListener("click", (e) => {
//    e.stopPropagation();
//    cartDropdown.classList.toggle("shown");
//});

// notification dropdown
const notificationBtn = document.querySelector(".header-btn.notifications");
const notificationDropdown = document.querySelector(".notification-dropdown");

notificationBtn.addEventListener("click", (e) => {
    e.stopPropagation();
    notificationDropdown.classList.toggle("shown");
});
// Card hover play pause
const cardVideo = Array.from(document.querySelectorAll(".card-video"));
const videoContainer = Array.from(document.querySelectorAll(".card-video-container"));

for (let i = 0; i < videoContainer.length; ++i) {
    videoContainer[i].addEventListener("mouseenter", () => {
        cardVideo[i].play();
    });

    videoContainer[i].addEventListener("mouseleave", () => {
        cardVideo[i].pause();
    });
}



