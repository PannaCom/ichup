var isLoadedCat = false;
var acategory = [];
var a1 = ["ảnh, ", "vector, ", "illustrator, "];
var a2 = ["dọc, ", "ngang, ", "rộng, "];

function getCategoryCk(id) {
    //console.log(acategory.length + "," + isLoadedCat);
    if (!isLoadedCat) {
        //console.log(acategory.length + "," + isLoadedCat);
        var xhr = new XMLHttpRequest();
        xhr.open('GET', '/Photos/getCategoryCk');
        xhr.send();
        var content = "";
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                if (xhr.responseText != "") {
                    var news = '{"news":' + xhr.responseText + '}';
                    var json_parsed = $.parseJSON(news);
                    isLoadedCat = true;
                    $("#category_ck_list_" + id).html("");
                
                        for (var i = 0; i < json_parsed.news.length; i++) {
                            if (json_parsed.news[i]) {
                                var name = json_parsed.news[i];
                                //alert(name);
                                var index = i + 1;
                                $("#category_ck_list_" + id).append("<input value='" + name + "' id=f-" + id + "-3_" + index + " type=checkbox>" + name);
                                //if (!isLoadedCat) acategory.push(name);
                                //console.log(acategory.length + "," + isLoadedCat);
                            }
                        }
                

                    
                

                } else {
                    //alert("Chương trình đang cập nhật, xin quay lại sau!");
                }
            }//status
        }//xhr
    } else {
        $("#category_ck_list_" + id).html("");
        for (var i = 0; i < acategory.length; i++) {
            var name = acategory[i];
            //alert(name);
            var index = i + 1;
            $("#category_ck_list_" + id).append("<input value='" + name + "' id=f-" + id + "-3_" + index + " type=checkbox>" + name);

        }
    }
}
function checkPrice(obj) {
    if (isNaN(obj.value)) obj.value = parseInt(obj.value);
}
function formatCurrency(number) {
    if (number == null || number == "") return 0;
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}