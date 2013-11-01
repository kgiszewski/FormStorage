$(function () {

    $(".search").click(function () {
        getSubmissions();
    });

    $(document).keypress(function (e) {
        if (e.which == 13) {
            $('.search').click();
            e.preventDefault();
        }
    });

    $(".download").click(function () {
        window.location.href = "/App_Plugins/FormStorage/download.aspx?keywords=" + escape($(".keywords").val()) + "&maxResults=" + $(".maxResults").val() + "&formAlias=" + $(".FormStorageWrapperDiv").attr('alias') + "&occurring=" + $(".occurring").val();
    });

    //ini
    stripe();

    //click the search on page load
    $('.search').click();

    //get some whitespace back
    $(".FormStorageWrapperDiv").closest(".propertyItem").find(".propertyItemheader").hide();
});

function escape(string){
    return (encodeURI(string));
}

function getSubmissions(){
  $.ajax({
      type: "POST",
      async: false,
      url: "/App_Plugins/FormStorage/FormStorageWebService.asmx/GetSubmissions",
      data: '{"keywords":"' + escape($(".keywords").val()) + '", "maxResults":' + $(".maxResults").val() + ', "formAlias":"' + $(".FormStorageWrapperDiv").attr('alias') + '", "occurring":"' + $(".occurring").val() + '"}',
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (returnValue){
        var response=returnValue.d;
        //console.log(response);
        
        switch(response.status){
          case 'SUCCESS':
              updateResults(eval(response.entries));
              break;
          case 'ERROR':
              var $tbody=$('.resultsDiv tbody');
              $tbody.html('');
              
              $tbody.append("<tr><td>"+response.message+"</td></tr>");
              break;
        }
      }
  });
}

function deleteSubmission($tr) {
    $.ajax({
        type: "POST",
        async: false,
        url: "/App_Plugins/FormStorage/FormStorageWebService.asmx/DeleteSubmission",
        data: '{"submissionID":"' + $tr.attr('submissionID') + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returnValue) {
            var response = returnValue.d;
            //console.log(response);

            switch (response.status) {
                case 'SUCCESS':
                    $tr.hide();
                    break;
                case 'ERROR':
                    alert(response.message);
                    break;
            }
        }
    });
}

function updateResults(entries){
    var $thead=$('.resultsDiv thead');
    var numCols=$thead.find('th').length;
    
    var $tbody=$('.resultsDiv tbody');
    $tbody.find('tr').remove();
    
    if(entries.length>0){
        
        for(var i=0;i<entries.length;i++){
            var $newTR=$("<tr submissionID='"+entries[i].ID+"'></tr>");
            $tbody.append($newTR);
            
            var $newTD;
            
            $newTD=$("<td></td>");
            $newTR.append($newTD);
            $newTD.html(entries[i].dateTime);
            
            $newTD=$("<td></td>");
            $newTR.append($newTD);
            $newTD.html(entries[i].IP);
            
            for(var j=0;j<entries[i].values.length;j++){
                $newTD=$("<td></td>");
                $newTR.append($newTD);
                $newTD.html("<div class='textContainer'>"+entries[i].values[j]+"</div>");
            }

            $newTD = $("<td><img class='delete' src='/App_Plugins/FormStorage/images/minus.png'/></td>");
            $newTR.append($newTD);
        }
        
        $('.delete').click(function(){
            
            var $button=$(this);
            var $tbody=$button.closest('tbody');
            var $tr=$button.closest('tr');
            
            if(confirm('Are you sure you wish to remove this?  This cannot be undone.')){
                deleteSubmission($tr);
            }
        });

        $('.FormStorageWrapperDiv tr').click(function () {

            var $thisTR = $(this);

            var height = ($thisTR.hasClass("open")) ? '40px' : 'auto';
            $thisTR.toggleClass("open");

            var $divs = $thisTR.find(".textContainer");

            $divs.each(function () {
                $(this).height(height);
            });
           
        });
        
    } else {
        var $newTR=$("<tr></tr>");
        $tbody.append($newTR);
        
        var $newTD=$("<td colspan='"+numCols+"'>No results found.</td>");
        $newTR.append($newTD);
    }
    
    stripe();
}

function stripe(){
  $(".resultsDiv tr").not(':first').hover( 
    function () {
      $(this).addClass('rowHighlight');
    }, 
    function () {
      $(this).removeClass('rowHighlight');
    }
  );
}
