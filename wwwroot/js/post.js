function moveCommentBox(comment) {
    var commentID = comment != null ? comment.id.replace("comment-", "") : null;
    var replyDiv = document.getElementById("replyDiv");
    if (replyDiv != null) {
        var newReply = document.createElement("button");
        newReply.classList.add("btn");
        newReply.classList.add("btn-primary");
        newReply.classList.add("reply");

        var oldComment = replyDiv.closest(".comment");

        if (oldComment == null) {
            newReply.textContent = "Comment";
            newReply.classList.add("m-3");

            document.getElementsByClassName("comments")[0].prepend(newReply);
        } else {
            newReply.textContent = "Reply";
            replyDiv.closest(".comment").appendChild(newReply);
        }
        replyDiv.remove();
    }


    $.get("/Comment/Create", { postID: $(".post").attr("id"), commentID: commentID }, function (data) {
        if (comment == null) {
            $(".comments").prepend(data).append(function () {
                $("#replyForm").validate();
            });
        } else {
            $(comment).append(data).append(function () {
                $("#replyForm").validate();
                $("#replyForm button").html("Reply");
            });
        }
        $("#replyForm .recaptcha").click(function (e) {
            var form = $("#replyForm");
            if (!form.valid()) {
                return;
            }

            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();

            grecaptcha.ready(function () {
                grecaptcha.execute(siteKey, { action: "submit" }).then(function (token) {
                    document.getElementsByName("ReCaptchaResponse")[0].value = token;

                    $("#replyForm").removeData("validator").removeData("unobtrusiveValidation");//remove the form validation
                    $.validator.unobtrusive.parse($("#replyForm"));//add the form validation

                    if ($("#replyForm").valid()) {
                        $("#replyForm").submit();
                    }
                });
            });
        });
    });

    $(".reply").unbind("click").click(function () {
        moveCommentBox(this.closest(".comment"));
        this.remove();
    });
}

document.addEventListener('DOMContentLoaded', () => {
    moveCommentBox();
});