<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes"/>
	<xsl:template match="article">
		<html>
			<head>
				<title><xsl:value-of select="@title"/></title>
				<style>
					<xsl:call-template name="Styles"/>
				</style>
				<script language="javascript">
             <![CDATA[ 
           %SCRIPT%
           ]]>
				</script>
			</head>
			<body leftmargin="0" rightmargin="0" topmargin="0" style="margin-left:0px;margin-right:0px;margin-top:0px;">
				<xsl:variable name="arr" select="generate-id()"/>
				<xsl:variable name="id" select="generate-id()"/>
				<div class="pageHeader"><xsl:value-of select="@title"/></div>

        <div style="padding-left:10px;padding-right:10px;padding-bottom:10px">
        <!--table cellpadding="0" cellspacing="0" style="margin:0px;width:200px">
						<tr>
							<td style="padding-left:3px;cursor:pointer" onclick="toggleExpand('{$arr}', '{$id}')">
								<xsl:call-template name="Arrow">
									<xsl:with-param name="id" select="$arr"/>
								</xsl:call-template>
							</td>
							<td class="tocTableTitle" style="cursor:pointer" onclick="toggleExpand('{$arr}', '{$id}')">Table of Contents</td>
						</tr>
					</table>
					<table id="{$id}" cellpadding="0" cellspacing="0" style="margin:0px;border:dotted 1px darkgray;background-color:#F9F9F9;display:none">						
						<tr>
							<td class="tocTable">
								<xsl:for-each select="//section">
									<div style="padding-left:{(number(@level)-1)*15}px">
										<a href="#{@title}">
											<xsl:if test="@level='1'"><xsl:attribute name="style">font-weight:bold</xsl:attribute></xsl:if>
											<xsl:value-of select="@title"/>
										</a>
									</div>
								</xsl:for-each>
							</td>					
						</tr>
					</table-->
          <xsl:apply-templates />
        </div>
        
			</body>
		</html>		
	</xsl:template>
			
	<xsl:template match="br">
    <div style="padding-top:5px">
      <xsl:apply-templates/>
    </div>
	</xsl:template>
	
	<xsl:template match="key">
		<code><xsl:apply-templates/></code>
	</xsl:template>
	
	<xsl:template match="section">
		<xsl:if test="@level='1'"><h1><xsl:value-of select="@title"/></h1></xsl:if>
		<xsl:if test="@level='2'"><h2><xsl:value-of select="@title"/></h2></xsl:if>
		<xsl:if test="@level='3'"><h3><xsl:value-of select="@title"/></h3></xsl:if>
		<xsl:if test="@level='4'"><h4><xsl:value-of select="@title"/></h4></xsl:if>
		<a name="{@title}"></a>
	</xsl:template>   
	
	<xsl:template match="b">
		<b><xsl:apply-templates/></b>
	</xsl:template>
	
	<xsl:template match="row">
		<table class="textTable">
			<tr>
				<xsl:apply-templates/>
			</tr>
		</table>
	</xsl:template>
	
	<xsl:template match="table">
		<table class="textTable"><xsl:apply-templates/></table>
	</xsl:template>
	
	<xsl:template match="td">
		<td class="textTd" width="{@width}"><xsl:apply-templates/></td>
	</xsl:template>
	
	<xsl:template match="tdn">
		<td class="textTd" style="text-align:right" width="{@width}"><xsl:apply-templates/></td>
	</xsl:template>
	
	<xsl:template match="th">
		<th class="textTh" width="{@width}"><xsl:apply-templates/></th>
	</xsl:template>
	
	<xsl:template match="tr">
		<tr><xsl:apply-templates/></tr>
	</xsl:template>
	
	<xsl:template match="i">
		<i><xsl:apply-templates/></i>
	</xsl:template>
	
	<xsl:template match="list">
		<ol>
			<xsl:apply-templates/>
		</ol>
	</xsl:template>
	
	<xsl:template match="li">
		<li>
			<xsl:apply-templates/>
		</li>
	</xsl:template>

  <xsl:template match="a">
    <a href="{@href}">
      <xsl:apply-templates/>
    </a>
  </xsl:template>
	
	<xsl:template match="code">
		<xsl:variable name="id" select="generate-id()"/>
		<pre id="{$id}" onload="alert(DoJava(this.innerHTML))" tab-width="2"><xsl:apply-templates/></pre>
		<script language="javascript">
			var el = document.getElementById('<xsl:value-of select="$id"/>');
      var str = el.innerHTML;
      str = DoKeywords(str);
      str = DoComment1(str);
      str = DoComment2(str);
      str = DoString(str);
      el.innerHTML = str;
    </script>
	</xsl:template>

  <xsl:template match="pre">
    <xsl:variable name="id" select="generate-id()"/>
    <pre id="{$id}" onload="alert(DoJava(this.innerHTML))" tab-width="2">
      <xsl:apply-templates/>
    </pre>
    <script language="javascript">
      var el = document.getElementById('<xsl:value-of select="$id"/>');
      var str = el.innerHTML;
      str = DoProcess(str);
      el.innerHTML = str;
    </script>
  </xsl:template>
	
	<xsl:template match="link">
		<a href="{@url}"><xsl:apply-templates/></a>
	</xsl:template>
	
	<xsl:template name="Arrow">
		<xsl:param name="id" />
		<table id="down_{$id}" cellpadding="0" cellspacing="0" style="width:7px;display:none">
			<tr>
				<td class="wc">.</td><td class="bc">.</td><td class="bc">.</td><td class="bc">.</td>
				<td class="bc">.</td><td class="bc">.</td><td class="wc">.</td>
			</tr>
			<tr>
				<td class="wc">.</td><td class="wc">.</td><td class="bc">.</td><td class="bc">.</td>
				<td class="bc">.</td><td class="wc">.</td><td class="wc">.</td>
			</tr>
			<tr>
				<td class="wc">.</td><td class="wc">.</td><td class="wc">.</td><td class="bc">.</td>
				<td class="wc">.</td><td class="wc">.</td><td class="wc">.</td>
			</tr>
		</table>
		<table id="up_{$id}" cellpadding="0" cellspacing="0" style="width:7px">
			<tr>
				<td class="wc">.</td><td class="wc">.</td><td class="bc">.</td><td class="bc">.</td>
				<td class="bc">.</td><td class="wc">.</td><td class="wc">.</td>
			</tr>
			<tr>
				<td class="wc">.</td><td class="bc">.</td><td class="bc">.</td><td class="bc">.</td>
				<td class="bc">.</td><td class="bc">.</td><td class="wc">.</td>
			</tr>
			<tr>
				<td class="bc">.</td><td class="bc">.</td><td class="bc">.</td><td class="bc">.</td>
				<td class="bc">.</td><td class="bc">.</td><td class="bc">.</td>
			</tr>
		</table>
	</xsl:template>
	
	<xsl:template name="Styles">
	body
	{
		font-family:Segoe UI;
		font-size:9pt;
	}
	
	h1
	{
		font-size:14pt;
		font-weight:bold;
		margin-top:20px;		
		margin-bottom:5px;
    color:gray;
	}
	
	h2
	{
		font-size:9pt;
		font-weight:bold;
		margin-top:12px;		
		margin-bottom:5px;
	}
		
	h3 
	{
		font-size:8pt;
		font-weight:bold;
		margin-top:9px;		
		margin-bottom:3px;
	}
	
	h4
	{
		font-size:9pt;
		font-weight:bold;
	}
	
	td.tocTable
	{
		padding:6px 6px 6px 6px;
		font-size:8pt;
		width:200px;
	}
	
	td.tocTableTitle
	{
		font-size:8pt;
		font-weight:bold;
		padding-left:6px;
		width:190px;
	}
	
	td.tocTableIcon
	{
		width:10px;
		text-align:center;
		font-size:8pt;
		font-weight:bold;
		border:solid 1px darkgray;
		padding:0px;
	}
	
	div.pageHeader
	{
		font-size:16pt;
		font-weight:bold;
		margin-bottom:15px;
		background-color:#EEEEEE;
		padding:5px;
		padding-left:10px;
		border-bottom:dashed 1px darkgray;
	}
		
	pre
	{  
		font-family:Consolas;
		font-size:8pt;
		margin-left:0px;		
		margin-top:7px;
		margin-bottom:7px;
		margin-right:50px;
		padding:5px 5px 5px 5px;		
		background-color:#F3F3F3;		
		
		margin-left:20px;
		border-left:solid 3px #C0C0C0;
	}
	
	code
	{
		font-family:Consolas;
		font-size:8pt;
		background-color:#F3F3F3;
	}
	
	a,a:hover,a:active,a:visited
	{
		color:navy;
		text-decoration:none;
	}
	
	a:hover
	{
		text-decoration:underline;
	}
  	
	.kw	
	{
		color:blue;
	}
	
	.str
	{
		color:brown;
	}
	
	.com
	{
		color:green;
	}
	
	ol
	{
		margin-top:5px;
		margin-bottom:5px;
	}
	
	.bc
	{
		width:1px;
		font-size:1px;
		color:transparent;
		background-color:black;
	}
	
	.wc
	{
		width:1px;
		font-size:1px;
		color:transparent;
		background-color:transparent;
	}
	
	.textTable
	{
		border-collapse:collapse;
		border:solid 1px darkgray;	
		margin-top:5px;
		margin-bottom:5px;
	}
	
	.textTh
	{
		background-color:#D7D7D7;
		font-size:8pt;
		font-weight:bold;
		font-family:Segoe UI;
		border:solid 1px darkgray;		
		padding:2px 2px 2px 2px;
	}
	
	.textTd
	{
		font-size:8pt;
		font-family:Segoe UI;
		border:solid 1px darkgray;		
		padding:2px 2px 2px 2px;
	}
  
  table 
  {
  	margin-left:0px;		
		margin-top:7px;
		margin-bottom:7px;
		margin-right:50px;
  }
	</xsl:template>
</xsl:stylesheet>
