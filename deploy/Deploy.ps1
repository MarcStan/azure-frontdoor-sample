<#
.SYNOPSIS

Helper script to deploy the frontdoor infrastructure to Azure.

.PARAMETER AppServicePlanResourceGroupName

Name of the resourcegroup where the existing app service plans are located.

.PARAMETER ResourceGroupName

Name of the resourcegroup to deploy. Name of resources will be derived from it.

#>

param(
    [Parameter(Mandatory=$true)]
    [string] $WebApp1,
    [Parameter(Mandatory=$false)]
    [string] $WebApp1Location = "westeurope",
    [Parameter(Mandatory=$false)]
    [string] $AppService1 = "appserviceplan1",
    [Parameter(Mandatory=$true)]
    [string] $WebApp2,
    [Parameter(Mandatory=$false)]
    [string] $WebApp2Location = "centralus",
    [Parameter(Mandatory=$false)]
    [string] $AppService2 = "appserviceplanus",
    [Parameter(Mandatory=$true)]
    [string] $AppServicePlanResourceGroupName,
    [Parameter(Mandatory=$true)]
    [string] $ResourceGroupName,
    [string] $ResourceGroupLocation = "westeurope"
)
$ErrorActionPreference = "Stop"

$ResourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if ($ResourceGroup -eq $null) {
    Write-Output "Creating resourcegroup $ResourceGroupName in $ResourceGroupLocation"
    New-AzResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Force -ErrorAction Stop
}

$ase1 = Get-AzAppServicePlan -ResourceGroupName $AppServicePlanResourceGroupName -Name $AppService1
if ($ase1 -eq $null) {
    Write-Error "App Service Plan $AppService1 not found in resourcegroup $AppServicePlanResourceGroupName"
}
$ase2 = Get-AzAppServicePlan -ResourceGroupName $AppServicePlanResourceGroupName -Name $AppService2
if ($ase2 -eq $null) {
    Write-Error "App Service Plan $AppService2 not found in resourcegroup $AppServicePlanResourceGroupName"
}

Write-Output "Deploying resourcegroup $ResourceGroupName"

$templateFile = Join-Path $PSScriptRoot "deploy.json"
$parameters = @{
    "webApp1" = $WebApp1
    "webApp1Location" = $WebApp1Location
    "appServicePlan1Name" = $AppService1
    "webApp2" = $WebApp2
    "webApp2Location" = $WebApp2Location
    "appServicePlan2Name" = $AppService2
    "appServicePlanResourceGroupName" = $AppServicePlanResourceGroupName
}

New-AzResourceGroupDeployment `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile $templateFile `
    -TemplateParameterObject $parameters `
    -Mode Complete `
    -Force
