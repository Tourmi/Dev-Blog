document.addEventListener('DOMContentLoaded', () => {
    $("#subscribeForm .recaptcha").click(function (e) {
        var form = $("#subscribeForm");
        if (!form.valid()) {
            return;
        }

        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();

        grecaptcha.ready(function () {
            grecaptcha.execute(siteKey, { action: "submit" }).then(function (token) {
                document.getElementsByName("ReCaptchaResponse")[0].value = token;

                $("#subscribeForm").removeData("validator").removeData("unobtrusiveValidation");
                $.validator.unobtrusive.parse($("#subscribeForm"));

                if ($("#subscribeForm").valid()) {
                    $("#subscribeForm").submit();
                }
            });
        });
    });
});