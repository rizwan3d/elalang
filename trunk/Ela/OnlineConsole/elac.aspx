<%@ Page Language="C#" ValidateRequest="false" %>
<%@ Assembly Name="Ela" %>
<html>
	<head>
		<title>Ela Online Interactive Console</title>
		<style>
		.topTable
		{
			width:100%;
			height:100%;
			border:none;
		}
		
		.bannerCell
		{
			color:white;
			background-color:#515151;
			font-family:Verdana,Arial;
			font-size:16pt;
			font-weight:bold;
			text-align:center;
			height:50px;
		}
		
		.footerCell
		{
			color:white;
			background-color:#515151;
			font-family:Verdana,Arial;
			font-size:7pt;
			font-weight:bold;
			text-align:center;
			height:20px;
			text-align:left;
		}
		
		.versionDiv
		{
			padding-top:3px;
			color:white;
			font-family:Verdana,Arial;
			font-size:8pt;
		}
		
		.marginCell
		{
		
		}
		
		.middleCell
		{
			width:640px;
			vertical-align:top;
		}
		
		.consoleTable
		{
			margin-top:20px;
			width:640px;
			border:none;
		}
		
		.execButtonCell
		{
			padding-top:10px;
		}
		
		.statCell
		{
			width:350px;
			padding-top:10px;
			font-family:Verdana,Arial;
			font-size:12pt;
			color:#515151;
			font-weight:bold;
			text-align:left;
			vertical-align:center;
		}
		
		.console
		{
			font-family:Courier New;
			font-size:9pt;
			border:solid 1px darkgray;
			width:100%;
			height:50px;
		}
		
		.execButton,.execButtonHover
		{
			font-family:Verdana,Arial;
			font-size:12pt;
			font-weight:bold;
			color:#515151;
			background-color:#D6D6D6;
			border:solid 1px #515151;
			padding:2px 2px 2px 2px;
			cursor:pointer;
			width:140px;
			height:40px;
		}
		
		.execButtonHover
		{
			color:#D6D6D6;
			background-color:#515151;
		}
		
		.resultTable
		{
			margin-top:20px;
			width:640px;
			border:none;
			border-collapse:collapse;
		}
		
		.resultCell
		{
			border:solid 1px darkgray;
			height:300px;
			max-height:300px;
			width:640px;
			max-width:640px;
			vertical-align:top;
		}
		
		.resultDiv
		{
			font-family:Courier New;
			font-size:9pt;	
			height:300px;
			max-height:300px;
			width:640px;
			max-width:640px;			
			overflow:auto;			
		}
		
		.err
		{
			color:red
		}
		
		.getLinkCell,.getLinkCellHover
		{
			border:solid 1px #515151;
			width:140px;
			height:40px;
			background-color:#D6D6D6;
			font-family:Verdana,Arial;
			font-size:12pt;
			font-weight:bold;
			color:#515151;
			text-align:center;
		}
		
		.getLinkCellHover
		{
			color:#D6D6D6;
			background-color:#515151;
			cursor:pointer;
		}
		
		a,a:hover,a:visited,a:active
		{
			color:#800000;
			text-decoration:none;
		}
		
		a:hover
		{
			text-decoration:underline;
		}
		</style>
		
		<script language="javascript">
		function createXMLHttpRequest() {
			var resObject = null;
			
			try {
				resObject = new ActiveXObject("Mircosoft.XMLHTTP");
			}			
			catch (error) {
				try {
					resObject = new ActiveXObject("MSXML2.XMLHTTP")
				}
				catch(error) {
					try {
						resObject = new XMLHttpRequest();
					}
					catch (error) {
						alert("XMLHttpRequest not available");
					}
				}
			}
			
			return resObject;
		}

		function execEla() {
			var src = document.getElementById("console").value;
            document.getElementById("console").value = "";
			start();
			var req = createXMLHttpRequest();
			req.open("POST", "/exec.aspx?tag=" + getTag());
            req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            var frm = "src=" + encodeURIComponent(src);
            
			req.onreadystatechange = function() {
				if (req.readyState == 4) {
					var xml = req.responseXML;
					var root = null;
					
					if (typeof(xml.selectSingleNode) != "function")
						root = xml.getElementsByTagName("Result")[0];
					else
						root = xml.selectSingleNode("//Result");
                        
                    if (root != null) {
						var div = document.getElementById("resultDiv");
						var apd = "";			
			
						if (root.getAttribute("Type") == "Timeout")
							apd = "<span class='err'>Your script timed out. Please don't use time consuming scripts.</span>" ;
						else if (root.getAttribute("Type") == "Overload")
							apd = "<span class='err'>Too many connections. Please try again later.</span>" ;
						else if (root.getAttribute("Type") != "Success")
							apd = "<span class='err'>" + root.getAttribute("Result") + "</span>";
						else
							apd = root.getAttribute("Result");
					
						div.innerHTML = apd + "<br><br>" + div.innerHTML;					
					}
					else	
						alert("Unexpected error occured!");
					
					end();
				}			
			};
			
			req.send(frm);			
		}
		
		function resetEla() {
			var src = document.getElementById("console").value;
			start();
			var req = createXMLHttpRequest();
			req.open("GET", "/exec.aspx?reset=1&tag=" + getTag());
			req.onreadystatechange = function() {
				if (req.readyState == 4) {
					var div = document.getElementById("resultDiv");
					div.innerHTML = "<span class='err'>Virtual machine reseted</span><br><br>" + div.innerHTML;
					end();
				}			
			};
			
			req.send(null);			
		}
		
		function getTag() {
			return new Date().toString();
		}
		
		function start() {
			document.getElementById("reset").style.visibility = "hidden";
			document.getElementById("exec").style.visibility = "hidden";
			var statCell = document.getElementById("statCell");
			statCell.innerHTML = "Executing...";
		}
		
		
		function end() {
			document.getElementById("reset").style.visibility = "visible";
			document.getElementById("exec").style.visibility = "visible";
			var statCell = document.getElementById("statCell");
			statCell.innerHTML = "&nbsp;";
		}
		</script>
	</head>
	<body style="margin:0px 0px 0px 0px">
		<table cellpadding="0" cellspacing="0" class="topTable">
			<tr>
				<td class="bannerCell" colspan="3">
				Ela Interactive Console
				<div class="versionDiv">
				using Ela <%= typeof(Ela.ElaMessage).Assembly.GetName().Version.ToString() %>
				</div>
				</td>
			</tr>
			<tr>
				<td class="marginCell">&nbsp;</td>
				<td class="middleCell">
					<table cellspacing="0" cellpadding="0" class="consoleTable">
						<tr>
							<td colspan="4">
								<textarea id="console" class="console"></textarea>
							</td>
						</tr>
						<tr>
							<td id="statCell" class="statCell">
								&nbsp;
							</td>
														
							<td class="execButtonCell">
								<input id="reset" type="button" onclick="resetEla()" 
									onmouseover="this.className='execButtonHover'" onmouseout="this.className='execButton'" 
									class="execButton" value="Reset"/>
							</td>
							
							<td style="font-size:1px;width:10px;">&nbsp;</td>
							<td class="execButtonCell">
								<input id="exec" type="button" onclick="execEla()" 
									onmouseover="this.className='execButtonHover'" onmouseout="this.className='execButton'" 
									class="execButton" value="Run!"/>
							</td>
						</tr>
					</table>
					
					<table cellpadding="0" cellspacing="0" class="resultTable">
						<tr>
							<td class="resultCell" colspan="2">
								<div id="resultDiv" class="resultDiv">&nbsp;</div>
							</td>
						</tr>
					</table>
					
					<div style="width:640px;font-family:Verdana,Arial;font-size:8pt;margin-top:10px;color:#515151">
					Enter Ela expressions in the upper text box and hit <i>Run!</i> to execute them. Ela will remember all your definitions, e.g.
					you can refer to variables and functions declared previously. In order to reset interpreter hit <i>Reset</i>. Please refer to the
					<a href="http://code.google.com/p/elalang/w/list">documentation</a> for more detail. You can download Ela <a href="http://code.google.com/p/elalang/downloads/list">here</a>.
					</div>
				</td>
				<td class="marginCell">&nbsp;</td>
			</tr>
			
			<tr>
				<td class="footerCell" colspan="3">
				Copyright Basil Voronkov 2011
				</td>
			</tr>
		</table>		
	</body>
</html>
<!--