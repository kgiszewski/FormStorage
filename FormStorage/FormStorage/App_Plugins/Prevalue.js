$(function () {

    //ini
    buildJson();

    $("input[type=text]").keyup(function () {
        buildJson();
    });

    function buildJson() {
        //console.log('building...');

        var $saveBox = $(".saveBox");

        var json = '';

        json += "{";
        json += "'alias' : '" + $(".alias").val() + "'";
        json += "}";

        $saveBox.val(json);
    }
});