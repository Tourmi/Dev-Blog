const getPaletteId = () => parseInt(document.body.className.substr(7), 10)

const changePalette = (id) =>
{
    document.body.className = 'palette' + id

    if (id == 1) {
        $(".fa-lightbulb").toggleClass("fas").toggleClass("far")
    } else if (id == 0) {
        $(".fa-lightbulb").toggleClass("far").toggleClass("fas")
    }
}

const select2Select = () => $(".tag-selector").select2({
    ajax: {
        url: "/tag/search",
        dataType: 'json'
    }
});


const select2Select2 = () => $(".tag-selector-post").select2({
    ajax: {
        url: "/tag/search?allowNewTags=true",
        dataType: 'json'
    }
});

function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

$(document).ready(function () {
    $("time.timeago").timeago()
    select2Select()
    select2Select2()

    //Fuck you Dashlane
    $(".select2-search__field").attr("data-form-type", "text")

    if (getPaletteId() == 0) {
        $(".fa-lightbulb").toggleClass("far").toggleClass("fas")
    }
    $("form").validate()
});