function openModal() {
    document.getElementById('galleryModal').style.display = "block";
}

function closeModal() {
    document.getElementById('galleryModal').style.display = "none";
}

var slideIndex = 0;
showSlides(slideIndex);

function plusSlides(n) {
    showSlides(slideIndex += n);
}

function currentSlide(n) {
    showSlides(slideIndex = n);
}

function showSlides(n) {
    var sources = $(".image-box");
    if (n < 0) { n = 0; }
    if (n > sources.length - 1) { n = sources.length - 1; }
    var next = $(sources[n]);
    var slide = $(".gallery-slide img");
    slide.attr("src", next.find("a").attr("href"));
}