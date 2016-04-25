$(document).ready(function () {

    $("#dropdown-menu-left").click(function () {
        $("#ul-dropdown-menu-left").toggle();

        var htmlString = $("#gallery-respon").attr('class');
        if (htmlString.trim() == "col-md-12") {
            $("#gallery-respon").removeClass("col-md-12").addClass("col-md-9 col-md-offset-3")

            $(".gallery-item").removeClass("col-xs-12 col-sm-6 col-md-3").addClass("col-xs-12 col-sm-6 col-md-4")

            $("#html-show-description").toggle();

        }
        if (htmlString.trim() == "col-md-9 col-md-offset-3") {
            $("#gallery-respon").removeClass("col-md-9 col-md-offset-3").addClass("col-md-12")

            $(".gallery-item").removeClass("col-xs-12 col-sm-6 col-md-4").addClass("col-xs-12 col-sm-6 col-md-3")
            $("#html-show-description").toggle();
        }
    });

    $("#dropdown-option-index").click(function () {
        $("#ul-dropdown-option-index").toggle();
    });

    $("#dropdown-option-index-1").click(function () {
        $("#ul-dropdown-option-index-1").toggle();
    });


    $("#input-search-index").keypress(function () {
        $("#ul-input-search-index").show();
    });

    $("body").click(function () {
        $("#ul-input-search-index").hide();
    });


    


    $('.img-resize').each(function () {

        $(this).css("height", $(this).width() * 190 / 270); // Ti le cua hinh anh
    });

    



});


$(window).resize(function () {

    $('.img-resize').each(function () {

        $(this).css("height", $(this).width() * 190 / 270);
    });
    
});