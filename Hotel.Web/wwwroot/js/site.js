document.querySelectorAll(".aviso button").forEach(function (b) {
    b.addEventListener("click", function () {
        b.closest(".aviso").remove();
    });
});
