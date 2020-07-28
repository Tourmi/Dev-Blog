const publishInput = document.querySelector('input[name="Publish"]');
const scheduleInput = document.querySelector('input[name="Schedule"]');
const autoGenerateStubInput = document.querySelector('input[name="AutoGenerateStub"]');
const contentInput = document.querySelector('textarea[name="Content"]');
const stubInput = document.querySelector('input[name="Stub"]');
const titleInput = document.querySelector('input[name="Title"]');
const filesInput = document.getElementById("post-files");
const updatePreviewBtn = document.getElementById("update-preview")
const postPreview = document.getElementById("post-preview");

var isStubGeneratedOnTitleUpdate = true;

const updatePublishDate = () => document.querySelector('input[name="PublishDate"]').disabled = !publishInput.checked || !scheduleInput.checked;
const updateScheduleInput = () => scheduleInput.disabled = !publishInput.checked;
const updateStubAutoGeneration = () => {
    if (!autoGenerateStubInput.disabled) {
        isStubGeneratedOnTitleUpdate = autoGenerateStubInput.checked;
        document.querySelector('input[name="Stub"]').readOnly = isStubGeneratedOnTitleUpdate;
    }
};

const updateStub = () => {
    if (isStubGeneratedOnTitleUpdate) {
        var newStub = titleInput.value.replace(/\s+/g, '-').toLowerCase();
        newStub = newStub.replace(/^-a-z0-9/g, '');
        stubInput.value = newStub;
    }
};

const select2Select = () => $(".tag-selector").select2({
    ajax: {
        url: "/Tag/Search",
        dataType: 'json'
    }
});

function updatePreview() {
    $.get("/PostAdmin/PreviewContent", { content: contentInput.value }, function (data) {
        postPreview.innerHTML = data;
    });
}

function addFileReferences() {
    const fileList = Array.from(this.files);
    var currRef = startRef;
    var references = "";

    fileList.forEach(file => {
        var src = URL.createObjectURL(file);

        const regex = new RegExp("\\[" + currRef + "\\]: .*\\n", "g");
        var newRef = "[" + currRef + "]: " + src + "\n";

        //Is there already a reference with the currRef?
        if (contentInput.value.match(regex)) {
            //If so, replace it
            contentInput.value = contentInput.value.replace(regex, newRef);
        } else {
            //Otherwise, create a new one
            references += newRef;
        }

        if (file['type'].split('/')[0] === 'image') {
            contentInput.value += "\n![alt text][" + currRef + "]";

        } else if (file['type'].split('/')[0] === 'audio') {
            contentInput.value += "\n?[aria-label][" + currRef + "]";
        }
        currRef++;
    });

    contentInput.value = references + contentInput.value;
}

function createAudioElement(src) {
    var audio = document.createElement("audio");
    audio.setAttribute("controls", "controls");
    var source = document.createElement("source");
    source.setAttribute("src", src);
    audio.appendChild(source);
    return audio;
}

function createImageElement(src) {
    var image = document.createElement("img");
    image.setAttribute("src", src);
    return image;
}

publishInput.addEventListener('click', updateScheduleInput);
publishInput.addEventListener('click', updatePublishDate);
scheduleInput.addEventListener('click', updatePublishDate);
autoGenerateStubInput.addEventListener('click', updateStubAutoGeneration);
updatePreviewBtn.addEventListener('click', updatePreview);
titleInput.addEventListener('input', updateStub);
filesInput.addEventListener('change', addFileReferences);

document.addEventListener('DOMContentLoaded', () => {
    updateScheduleInput();
    updatePublishDate();
    updateStubAutoGeneration();
    select2Select();
    isStubGeneratedOnTitleUpdate = autoGenerateStubInput.checked;
});
