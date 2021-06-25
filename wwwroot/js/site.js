function changePalette(id) {
    document.body.className = 'palette' + id;
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

$(document).ready(function () {
    $("time.timeago").timeago();
    select2Select();
    select2Select2();
});