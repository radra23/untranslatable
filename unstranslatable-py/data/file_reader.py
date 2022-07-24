from os.path import dirname, join
import json
    
def read_json_from_file():
    here = dirname(__file__)
    with open(join(here,"./data.json"), encoding="utf-8") as f:
        data = json.loads(f.read())
    return data
