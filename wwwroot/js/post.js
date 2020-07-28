function onSubmit(token) {
    document.getElementsByName("ReCaptchaResponse")[0].value = token;
    document.getElementById("replyForm").submit();
}

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
            document.getElementsByClassName("comments")[0].prepend(newReply);
        } else {
            newReply.textContent = "Reply";
            replyDiv.closest(".comment").appendChild(newReply);
        }
        replyDiv.remove();
    }


    $.get("/Comment/Create", { postID: $(".post").attr("id"), commentID: commentID }, function (data) {
        if (comment == null) {
            $(".comments").prepend(data);
        } else {
            $(comment).append(data);
        }
        $(".recaptcha").unbind("click").click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();

            grecaptcha.ready(function () {
                grecaptcha.execute(siteKey, { action: "submit" }).then(onSubmit);
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