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
function downloadimage() {
    var type = 1;

    if (document.getElementById("image_chk3").checked) {
        type = 3;
    } else if (document.getElementById("image_chk2").checked) {
        type = 2;
    }
    else if (document.getElementById("image_chk1").checked) {
        type = 1;
    }
    window.open("/Photos/downloadfile?id=" + currentId + "&type=" + type, "_self");
    //var formdata = new FormData(); //FormData object
    //formdata.append("id", currentId);
    //var xhr = new XMLHttpRequest();
    //xhr.open('POST', '/Photos/downloadfile');
    //xhr.send(formdata);
    //var content = "";
    //$("#loadingimage").show();
    //xhr.onreadystatechange = function () {
    //    if (xhr.readyState == 4 && xhr.status == 200) {
    //        if (xhr.responseText != "0") {
    //            $("#loadingimage").hide();
    //        } else {
    //            alert("Chương trình đang cập nhật, xin quay lại sau!");
    //        }
    //    }
    //}
}
function imagepage(id) {
    currentId = id;
    var formdata = new FormData(); //FormData object
    formdata.append("id", id);
    var xhr = new XMLHttpRequest();
    xhr.open('POST', '/Photos/getImage');
    xhr.send(formdata);
    var content = "";
    $("#loadingimage").show();
    $("#imagepage").show();
    $("#image_source").attr("src", "/Images/loading.gif");
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            if (xhr.responseText != "0") {
                //alert("Cập nhật thành công");
                //window.location.href = "/Photos/User/1";
                var news = '{"news":' + xhr.responseText + '}';
                var json_parsed = $.parseJSON(news);
                for (var i = 0; i < json_parsed.news.length; i++) {
                    var src = json_parsed.news[i].link_thumbail_big;
                    var id = json_parsed.news[i].id;
                    var price = json_parsed.news[i].price;
                    var image_width_height = json_parsed.news[i].width + "x" + json_parsed.news[i].height;
                    var image_keyword = json_parsed.news[i].keywords;
                    $("#image_source").attr("src", src);
                    $("#image_id").html("File ID - #" + id);
                    $("#image_price").html(price);
                    $("#image_price2").html("Giá: " + formatCurrency(price));
                    $("#image_width_height").html(image_width_height);
                    $("#image_keyword").html("Từ khóa: " + image_keyword);
                }
                $("#loadingimage").hide();
            } else {
                alert("Chương trình đang cập nhật, xin quay lại sau!");
            }
        }
    }
}