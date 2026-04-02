document.addEventListener('DOMContentLoaded', () => {
    const adminDiscountBtn = document.getElementById("admin-discount-btn");
    if (adminDiscountBtn) {
        adminDiscountBtn.addEventListener('click', adminDiscountClick);
    }
    else {
        console.error("#admin-discount-btn not found");
    }
});

function adminDiscountClick(e) {
    const form = e.target.closest("form");
    if (!form) throw "adminDiscountClick: Closest form not found";
    const formData = new FormData(form);
    fetch("", {
        method: "POST",
        body: formData
    }).then(r => r.json()).then(j => {
        console.log(j);
    });
}