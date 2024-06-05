function GetAntiXsrfRequestToken() {
    let csrf = $('[name="__RequestVerificationToken"]')[0].value;
    return csrf
}


$(document).on('click', '.removeBtn', function () {
    let csrf = GetAntiXsrfRequestToken();
    let exercisePartId = $(this).attr('exercisePartId');
    $.ajax({
        url: 'exercices/delete/' + exercisePartId,
        type: 'POST',
        headers: { "RequestVerificationToken": csrf },
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            document.location.reload();
        },
        error: function (req, status, error) {
            alert(error);
        }
    });
});