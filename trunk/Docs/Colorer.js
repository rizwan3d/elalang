function DoKeywords(code)
{
	return code
	.replace(/(\bbreak\b|\continue\b|\breturn\b|\byield\b|\bfor\b)/g,"<span class=kw>$1</span>")
	.replace(/(\bwhen\b|\bif\b|\belse\b|\blet\b|\bvar\b|\btype\b|b\cor\b)/g,"<span class=kw>$1</span>")	
	.replace(/(\btry\b|\bcatch\b|\bfinally\b|\bthis\b|\bbase\b|\bthrow\b)/g,"<span class=kw>$1</span>")	
	.replace(/(\btrue\b|\bfalse\b|\bmatch\b|\bthrow\b|\bwith\b|\bcout\b)/g,"<span class=kw>$1</span>")
	.replace(/(\r\n\r\n)/g, "<div>&nbsp;</div>")
	.replace(/(\n)/g, "<br>")
	.replace(/(\t)/g, "<span>&nbsp;&nbsp</span>")
	;
}

function DoComment1(code)   // // 
{
	code = code.replace(/(\/\/)(.*?)($)/gm,"<span class=com>$1$2$3</span>")
	return code;
}

function DoComment2(code)   // /* */
{
	code = code.replace(/(\/\*)((?:.|\n)*?)(\*\/)/gm,"<span class=com>$1$2$3</span>")
	return code;
}

function DoString(code, tp)
{
	code =
	   code.replace(/(@"(:?(:?""|[^"])*)"|"(:?(:?\\"|[^"])*)")/g,"<span class=str>$1</span>")
	return code;
}

function toggleExpand(arrKey, elKey) {
	var el = document.getElementById(elKey);
	var darr = document.getElementById("down_" + arrKey);
	var uarr = document.getElementById("up_" + arrKey);

	if (el.style.display == "none") {
		darr.style.display = "inline";
		uarr.style.display = "none";
		el.style.display = "inline";
	}
	else {
		darr.style.display = "none";
		uarr.style.display = "inline";
		el.style.display = "none";
	}
}