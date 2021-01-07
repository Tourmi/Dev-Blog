function changePalette(id) {
    document.body.className = 'palette' + id;
}

$(document).ready(function () {
    $("time.timeago").timeago();
});