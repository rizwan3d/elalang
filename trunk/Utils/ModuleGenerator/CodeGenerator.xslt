<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
<xsl:output omit-xml-declaration="yes"/>	

<xsl:template match="modules">
using System;
using Ela;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

	<xsl:for-each select="module">
		
namespace <xsl:value-of select="@namespace"/> 
{
<xsl:variable name="className" select="@className"/>
    partial class <xsl:value-of select="$className"/> : Ela.Linking.ForeignModule
	{
		public override void Initialize()
		{
		<xsl:for-each select="function">
			Add("<xsl:value-of select="@name"/>", new _elafunc_<xsl:value-of select="@methodName"/>(this));
		</xsl:for-each>
    
      AuxInitialize();
		}
    
    partial void AuxInitialize();

		<xsl:for-each select="function">
			<xsl:variable name="name" select="@methodName"/>
			<xsl:variable name="rt" select="@returnType"/>
			class _elafunc_<xsl:value-of select="@methodName"/> : ElaFunction
			{
				private <xsl:value-of select="$className"/> obj;
				internal _elafunc_<xsl:value-of select="@methodName"/>(<xsl:value-of select="$className"/> obj) : base(<xsl:choose><xsl:when test="@unlimitedPars='True'">-1</xsl:when><xsl:otherwise><xsl:value-of select="count(param)"/></xsl:otherwise></xsl:choose>)
				{
					this.obj = obj;
				}        
        
        protected override string GetFunctionName()
        {
          return "<xsl:value-of select="@name"/>";
        }
				
				<xsl:if test="contains($rt, '[]')">
					<xsl:call-template name="ConvertFromArray">
						<xsl:with-param name="type" select="$rt" />
					</xsl:call-template>
				</xsl:if>
				
				<xsl:for-each select="param">
					<xsl:choose>
						<xsl:when test="@type='Int32[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Int32'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Int64[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Int64'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Boolean[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Boolean'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Char[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Char'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='String[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'String'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Object[]'">
						  <xsl:call-template name="ConvertToArray">
							<xsl:with-param name="index" select="position()" />
							<xsl:with-param name="type" select="'Object'" />
						  </xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Double[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Double'" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="@type='Single[]'">
							<xsl:call-template name="ConvertToArray">
								<xsl:with-param name="index" select="position()" />
								<xsl:with-param name="type" select="'Single'" />
							</xsl:call-template>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>

				public override ElaValue Call(params ElaValue[] args)
				{
					try
					{
						<xsl:if test="$rt!='Void'">var ret =</xsl:if>        
						obj.<xsl:value-of select="$name"/>(
								<xsl:for-each select="param">
									<xsl:if test="position() &gt; 1">,</xsl:if>
									<xsl:choose>
										<xsl:when test="@type='ElaValue'">args[<xsl:value-of select="position()-1"/>]</xsl:when>
										<xsl:when test="@type='Int32'">
											args[<xsl:value-of select="position()-1"/>].AsInteger()
										</xsl:when>
										<xsl:when test="@type='Int64'">
											args[<xsl:value-of select="position()-1"/>].AsLong()
										</xsl:when>
										<xsl:when test="@type='Boolean'">
											args[<xsl:value-of select="position()-1"/>].AsBoolean()
										</xsl:when>
										<xsl:when test="@type='Char'">
											args[<xsl:value-of select="position()-1"/>].AsChar()
										</xsl:when>
										<xsl:when test="@type='String'">
											args[<xsl:value-of select="position()-1"/>].AsString()
										</xsl:when>
										<xsl:when test="@type='Double'">
											args[<xsl:value-of select="position()-1"/>].AsDouble()
										</xsl:when>
										<xsl:when test="@type='Single'">
											args[<xsl:value-of select="position()-1"/>].AsSingle()
										</xsl:when>
										<xsl:when test="@type='ElaObject'">
											(ElaObject)args[<xsl:value-of select="position()-1"/>].AsObject()
										</xsl:when>
										<xsl:when test="@type='ElaArray'">
											args[<xsl:value-of select="position()-1"/>].AsArray()
										</xsl:when>
										<xsl:when test="@type='ElaList'">
											args[<xsl:value-of select="position()-1"/>].AsList()
										</xsl:when>
										<xsl:when test="@type='ElaTuple'">
											args[<xsl:value-of select="position()-1"/>].AsTuple()
										</xsl:when>
										<xsl:when test="@type='ElaRecord'">
											args[<xsl:value-of select="position()-1"/>].AsRecord()
										</xsl:when>
										<xsl:when test="@type='ElaFunction'">
											args[<xsl:value-of select="position()-1"/>].AsFunction()
										</xsl:when>							
										<xsl:when test="@type='ElaValue[]'">
											args
										</xsl:when>
										<xsl:when test="contains(@type, '[]')">
											ConvertTo_<xsl:value-of select="position()"/>(args[<xsl:value-of select="position()-1"/>].AsArray())
										</xsl:when>
									</xsl:choose>
								</xsl:for-each>
								);							
					 return	<xsl:choose>
						<xsl:when test="$rt='ElaValue'">ret;</xsl:when>
						<xsl:when test="contains($rt, '[]')">ConvertFromArray(ret);</xsl:when>
						<xsl:when test="$rt='Void'">new ElaValue(ElaUnit.Instance);</xsl:when>
						<xsl:otherwise>new ElaValue(ret);</xsl:otherwise>
					</xsl:choose>
					}
					catch (ElaCastException ex)
					{
						if (ex.ExpectedType != ObjectType.None)
							throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
						else
							throw new ElaParameterTypeException(ex.InvalidType);
					}
				}
			}
		</xsl:for-each>
	}
}
	</xsl:for-each>
		
</xsl:template>

<xsl:template name="ConvertToArray">
	<xsl:param name="index" />
	<xsl:param name="type" />
		private <xsl:value-of select="$type"/>[] ConvertTo_<xsl:value-of select="$index"/>(ElaArray array)
		{
			var arr = new <xsl:value-of select="$type"/>[array.Length];

			<xsl:text disable-output-escaping="yes">for (var i = 0; i &lt; array.Length; i++)</xsl:text>
				arr[i] = array[i].As<xsl:value-of select="$type"/>();
			
			return arr;
		}
</xsl:template>
	
<xsl:template name="ConvertFromArray">
	<xsl:param name="type" />
		private ElaValue ConvertFromArray(<xsl:value-of select="$type"/> arr)
		{
			var array = new ElaArray(arr.Length);

			<xsl:text disable-output-escaping="yes">for (var i = 0; i &lt; arr.Length; i++)</xsl:text>
				array.Add(new ElaValue(arr[i]));

			return new ElaValue(array);
		}		
</xsl:template>
</xsl:stylesheet>
