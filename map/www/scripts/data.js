var data = (function ($) {
    "use strict";

    var data = {};

    function urlBase() {
        //if (ttApp.isMobile()) {
        //    return "http://www.quilkin.co.uk/Service1.svc/";
        //    // return "http://quilkin.azurewebsites.net/Service1.svc/"
        //}

        return "http://www.quilkin.co.uk/Service1.svc/";
        //return "http://localhost:59555/Service1.svc/";

    }
    function webRequestFailed(handle, status, error) {
        alert("Error ajax request: " + error);
    }

    data.json = function (url, type, data, successfunc, async) {
        var dataJson = JSON.stringify(data),
            thisurl = urlBase() + url;
        $.ajax({
            type: type,
            data: dataJson,
            url: thisurl,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: async,
            success: successfunc,
            error: webRequestFailed
        });
    };
    return data;


}(jQuery));
