$UTCTimeStamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddHHmmZ")
New-Item $UTCTimeStamp-NewScript.sql