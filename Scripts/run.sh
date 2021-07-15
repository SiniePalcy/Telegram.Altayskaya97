cd ~/git/Telegram.Altayskaya97
git pull
dotnet publish -r ubuntu.18.04-x64 -c Release
sudo systemctl restart bot.service
cd ~
