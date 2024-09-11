<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns:fabric="http://schemas.microsoft.com/2011/01/fabric"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    exclude-result-prefixes="fabric">
  <xsl:output method="xml" indent="yes" />
  <xsl:strip-space elements="*"/>

  <xsl:template match="@*| node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Declare Principals -->
  <xsl:template match="fabric:User[@Name='ServiceAccount']">
    <fabric:User Name="ServiceAccount" AccountType="ManagedServiceAccount" AccountName="COMPANY\EGGT__EC__T$" />
  </xsl:template>

</xsl:stylesheet>