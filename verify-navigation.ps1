$ErrorActionPreference='Stop'
$assembly=[Reflection.Assembly]::LoadFile((Join-Path $PSScriptRoot 'dist\Starfield.scr'))
$type=$assembly.GetType('Brough.Starfield.NavigationMath')
$heading=$type.GetMethod('Heading');$random=$type.GetMethod('RandomHeading');$stardate=$type.GetMethod('Stardate')
foreach($case in @(@(355.0,[single]0.1,13.0),@(5.0,[single]-.1,347.0),@(359.99,[single]0,0.0),@(90.0,[single]0,90.0))){
    if([Math]::Abs($heading.Invoke($null,@([double]$case[0],[single]$case[1]))-$case[2]) -gt .000001){throw 'Heading wrap or rounding failed'}
}
$headings=@(foreach($index in 1..128){$value=$random.Invoke($null,@());if($value -lt 0 -or $value -ge 360){throw 'Heading outside compass range'};$value}) | Sort-Object -Unique
if($headings.Count -lt 2){throw 'Starting headings are constant'}
foreach($case in @(@([DateTime]::new(2026,1,1),41000.0),@([DateTime]::new(2027,1,1),42000.0),@([DateTime]::new(2032,1,1),47000.0))){
    if([Math]::Abs($stardate.Invoke($null,@($case[0]))-$case[1]) -gt .000001){throw 'TNG-era calendar anchor failed'}
}
$start=[DateTime]::new(2026,11,2,12,0,0)
$value=$stardate.Invoke($null,@($start));$later=$stardate.Invoke($null,@($start.AddSeconds(40)))
if($value -lt 41000 -or $value -ge 42000 -or [Math]::Abs($later-$value-40000.0/(365*86400)) -gt .000001){throw 'Stardate clock progression failed'}
if($value.ToString('00000.000') -eq $later.ToString('00000.000')){throw 'Displayed stardate does not tick within 40 seconds'}
$yearEnd=$stardate.Invoke($null,@([DateTime]::new(2026,12,31,23,59,59)))
if($yearEnd -ge 42000 -or 42000-$yearEnd -gt .001){throw 'Stardate year rollover failed'}
$leap=$stardate.Invoke($null,@([DateTime]::new(2024,2,29)))
if([double]::IsNaN($leap) -or [double]::IsInfinity($leap)){throw 'Leap-day mapping failed'}
$beforeLeap=$stardate.Invoke($null,@([DateTime]::new(2024,2,28)))
$afterLeap=$stardate.Invoke($null,@([DateTime]::new(2024,3,1)))
if([Math]::Abs($leap-$beforeLeap-1000.0/366) -gt .000001 -or [Math]::Abs($afterLeap-$leap-1000.0/366) -gt .000001){throw 'Calendar does not advance evenly over leap day'}
$session=$assembly.GetType('Brough.Starfield.NavigationSession').GetField('Heading',[Reflection.BindingFlags]'Static,Public')
if($session.GetValue($null) -ne $session.GetValue($null)){throw 'Heading changes within a session'}
Write-Output 'Random heading range, compass wrap, session stability, TNG-era date anchors, clock ticks, year rollover and leap day passed.'
