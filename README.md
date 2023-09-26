# ImgOrganizr

## Overview

`ImgOrganizr` is a CLI tool designed to help manage your image files more effectively. It can rename and move image files based on metadata or other naming patterns, thereby making it easier to sort and find images.

## Flow
- **Backup**: Automatically backs up images before making any changes.
- **Extract Date Taken**: Extract 'Date Taken' from metadata, fallback to regex in filename, fallback to changed date.
- **Set Metadata**: Set the metadata creation datetime based on the 'Date Taken' extracted.
- **Rename Files**: Renames image files based on the creation datetime metadata.
- **Move Files**: Sorts image files into subdirectories based on their 'Date Taken' metadata.

## Installation

[Installation steps go here.]

## Usage

To use `ImgOrganizr`, navigate to the directory containing your images and execute with appropriate command-line arguments.

## Contributing

If you'd like to contribute, please fork the repository and submit a pull request.
