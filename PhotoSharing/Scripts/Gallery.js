var loaderImage = new Image();
slideIndex = 0;

function openModal() {
    document.getElementById('galleryModal').style.display = "block";
    showSlides(slideIndex);
}

function closeModal() {
    document.getElementById('galleryModal').style.display = "none";
}


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

    var href = next.find("a").attr("href");

    loaderImage.onload = function () {
        $("#loadingIndicator").hide();
        slide.attr("src", href);
    }

    $("#loadingIndicator").show();
    loaderImage.src = href;
}