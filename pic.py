from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.common.exceptions import TimeoutException
from anti_useragent import UserAgent
import os, sys, ipaddress

with open(sys.argv[1], "r") as f:
    iplis = f.readlines()
iplis = set([i.strip() for i in iplis if ipaddress.ip_address(i.strip()).version == 4])

os.makedirs(screenshot_folder := "pics", exist_ok=True)

options = Options()
options.add_argument("--headless")
options.add_argument("--incognito")
options.add_argument("--user-agent=" + str(UserAgent("windows").chrome))
#options.add_argument("--blink-settings=imagesEnabled=false")
options.add_argument("--no-default-browser-check")
options.add_argument("--disable-extensions")
options.add_argument("--disable-logging")

driver = webdriver.Chrome(options)
driver.set_page_load_timeout(30 if len(sys.argv) == 2 else int(sys.argv[2]))

counter = 0
for ip in iplis:
    print(f"{(counter := counter+1)} of {len(iplis)}...")
    if(os.path.isfile(file_path := os.path.join(screenshot_folder, f"{ip}.png"))): continue
    try:
        driver.get(f"http://{ip}")
        driver.save_screenshot(file_path)
        print(f"Saved screenshot of {ip} to {file_path}")
    except TimeoutException:
        print(f"Timeout while loading {ip}")
    except Exception as e:
        print(f"Error while loading {ip}: {e}")

driver.quit()
