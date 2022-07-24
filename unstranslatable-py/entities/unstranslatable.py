from dataclasses import dataclass

from dataclasses_json import Undefined, dataclass_json

@dataclass_json(Undefined.EXCLUDE)
@dataclass
class Unstranslatable:
    word : str
    language: str
    definition: str
    