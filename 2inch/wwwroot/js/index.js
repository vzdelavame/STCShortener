$(document).ready(function() {

    // Check for click events on the navbar burger icon
    $(".navbar-burger").click(function() {
  
        // Toggle the "is-active" class on both the "navbar-burger" and the "navbar-menu"
        $(".navbar-burger").toggleClass("is-active");
        $(".navbar-menu").toggleClass("is-active");
  
    });

    $("#edit-link").click(function() {
  
        var id = $("#idEdit").val();
        var shortLink = $("#shortLinkEdit").val();
        var longLink = $("#longLinkEdit").val();
        var owner = $("#ownerEdit").val();

        window.location = "/Admin/UpdateSelectedLink?id=" + id +"&shortLink=" + shortLink + "&longLink=" + longLink + "&owner=" + owner;
    });
});

function GetConfirmation() {
    if (confirm("Are you sure you want to delete the link?")) {
        return true;
    }
    else {
        return false;
    }
}