$(function () {

    var noteids = [];

    $("button[data-note-id]").each(function (i, e) {
        noteids.push($(e).data("note-id"));
    });
    console.log(noteids);
    $.ajax({
        method: "POST",
        url: "/Note/GetLiked",
        data: { "ids": noteids }
    }).done(function (data) {
    if (data.result != null && data.result.length > 0) {
                 for (var i = 0; i < data.result.length; i++) {
                 var id = data.result[i];
                     var likedNote = $("div[data-note-id=" + id + "]");
                     var btn = likedNote.find("button[data-liked]");
                     var span = btn.find("span.like-star");
                     console.log(span.length);
                     btn.data("liked", true);
                     span.removeClass("fa-heart-o");
                     console.log(span.length);
                     span.addClass("fa-heart");
                     console.log(span.length);
            }

        }

    }).fail(function () {

    });



    $("button[data-liked]").click(function () {
        var btn = $(this);
        var liked = btn.data("liked");
        var noteid = btn.data("note-id");
        var spanStar = btn.find("span.like-star");
        var spanCount = btn.find("span.like-count");

        console.log(liked);
        console.log("like count : " + spanCount.text());

        $.ajax({
            method: "POST",
            url: "/Note/SetLikedState",
            data: { "noteid": noteid, "liked": !liked }
        }).done(function (data) {

            console.log(data);

            if (data.hasError) {
                alert(data.errorMessage);
            } else {
                liked = !liked;
                btn.data("liked", liked);
                spanCount.text(data.result);

                console.log("like count(after) : " + spanCount.text());


                spanStar.removeClass("fa-heart-o");
                spanStar.removeClass("fa-heart");

                if (liked) {
                    spanStar.addClass("fa-heart");
                } else {
                    spanStar.addClass("fa-heart-o");
                }

            }

        }).fail(function () {
            alert("Sunucu ile bağlantı kurulamadı.");
        })

    });

});
