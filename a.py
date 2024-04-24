import sys, ipaddress, os
se = set()
with open(sys.argv[1], "r") as file:
    for line in file:
        tmp = line.strip().split()
        for i in tmp:
            if ipaddress.ip_address(i).version == 4:
                se.add(i)
chrome = "'" + os.path.abspath("C:\Program Files\Google\Chrome\Application\chrome.exe") + "'"
for i in se:
    print(i)
    a = "powershell Start-Process " + chrome + f' -ArgumentList "http://"{i}", "--incognito"'
    print(a)
    os.system(a)
    input("エンターキーを入力して続行")
