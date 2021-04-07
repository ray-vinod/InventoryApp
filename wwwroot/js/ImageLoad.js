//js funtion for all
var previewImage = function (event) {
    var result = document.getElementById("imgPreview");
    result.src = URL.createObjectURL(event.target.files[0]);
};