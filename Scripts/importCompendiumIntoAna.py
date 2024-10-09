# © Anamnesis.
# Licensed under the MIT license.
import pandas as pd
import sqlite3
import json

# The Compendium of Non-Weapon Held Objects is a community project
# available here: https://docs.google.com/spreadsheets/d/10BBOwsbhIx4F7SUvZPX4gor6CYIkZBLLSv1Mvn2VRo0
# It aims to be a comprehensive prop table for in game items.
# we aim to import this table into the Anamnesis Special equipment list

# Before we begin, we need to export the Compendium as an xlsx. 
# The idea is to build a pair of sql tables out of the xlsx Compendium export and
# the currently existing Anamnesis Special Equipment json array
# in order to more easily compare them and export them
connection = sqlite3.connect(":memory:")
cursor = connection.cursor()
compendiumFilename = "Compendium of Non-Weapon Held Objects.xlsx"
AnamnesisSpecialEquipmentFilename = "Equipment.json"

# First we build a table out of the xlsx Compendium export file
compendiumDataframe = pd.read_excel(compendiumFilename, sheet_name="Props")
compendiumDataframe.to_sql("compendium", connection, index=False, if_exists="replace")
connection.commit()

# Then we build a second table out of the Anamnesis Equipment array
AnaDataframe = pd.read_json(AnamnesisSpecialEquipmentFilename)
# Splits the Id field into Set, Base, Variant. Fills with None if less than 3 fields
AnaDataframe[["Set", "Base", "Variant"]] = AnaDataframe["Id"].str.split(
    ", ", n=3, expand=True
)
AnaDataframe.to_sql("anamnesis", connection, index=False, if_exists="replace")
connection.commit()

# Finds all records in Compendium that do not currently match on IDs on Anamnesis
resultDataFrame = pd.read_sql(
    """
                     SELECT compendium.'Item Name', compendium.'First Value (Set)', compendium.'Second Value (Base)', compendium.'Variant', compendium.'Wields to', compendium.'Alt. Name/Description'
                     FROM compendium
                     LEFT OUTER JOIN anamnesis
                     ON (anamnesis.'Set' IS compendium.'First Value (Set)') AND (anamnesis.'Base' IS compendium.'Second Value (Base)') AND (anamnesis.'Variant' IS compendium.'Variant')
                     WHERE anamnesis.'Set' IS NULL
    """,
    connection,
)

# slotMapper maps between the compendium's 'Wields to' and Anamnesis' FitsSlots
slotMapper = {
    None: None,
    "hand": "Weapons",
    "Head": "Head",
    "head": "Head",
    "Offhand": "OffHand",
    "Root": "Weapons",
}
# Build an array of objects to write to file
objectsToAddToAnamnesis = []

for index, row in resultDataFrame.iterrows():
    # Items which do not have a third slot build their ids differently
    if not pd.isna(row["Variant"]):
        resultId = (
            str(row["First Value (Set)"])
            + ", "
            + str(row["Second Value (Base)"])
            + ", "
            + str(int(row["Variant"]))
        )
    else:
        resultId = (
            str(row["First Value (Set)"]) + ", " + str(row["Second Value (Base)"])
        )

    # Each object in the Json has at least Name, Id and Slot
    resultRow = {
        "Name": row["Item Name"],
        "Id": resultId,
        "Slot": slotMapper[row["Wields to"]],
    }
    # To avoid Null values in Json, we take only the non-null descriptions
    if row["Alt. Name/Description"]:
        resultRow["Description"] = row["Alt. Name/Description"]

    # If the row has a Slot, we can just add it to the resulting object
    if resultRow["Slot"] != None:
        objectsToAddToAnamnesis.append(resultRow)
    else:
        # If the current row has no Slot, but has a Variant, it's probably a Weapon snap
        # as other slots do not have a Variant
        if not pd.isna(row["Variant"]):
            resultRow["Slot"] = "Weapons"
            objectsToAddToAnamnesis.append(resultRow)
        # Unfortunately, we cannot assume that a slot that does not have a Variant goes in any particular other slot

with open("EquipmentInCompendiumAndNotInAnamnesis.json", "w") as resultFile:
    json.dump(objectsToAddToAnamnesis, resultFile, indent=2)

print("New EquipmentInCompendiumAndNotInAnamnesis.json generated")
