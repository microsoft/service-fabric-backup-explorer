# -----------------------------------------------------------------------------
# Adapted from Microsoft OSS
# see https://github.com/Microsoft/service-fabric-cli
# -----------------------------------------------------------------------------

import setuptools

with open("README.md", "r") as fh:
    long_description = fh.read()


setuptools.setup(
    name='bkpctl',
    version='1.0.0',
    description='Azure Service Fabric Reliable Collections Backup Explorer Command Line',
    long_description=long_description,
    long_description_content_type="text/markdown",
    url='https://github.com/microsoft/service-fabric-backup-explorer',
    author='Aakar Gupta',
    author_email='aagup@microsoft.com',
    license='MIT',
    keywords='servicefabric azure',
    python_requires='>=2.7,!=3.4,!=3.3,!=3.2,!=3.1,!=3.0,<3.9',
    packages=[
        'bkpctl',
        'bkpctl.helps'
    ],
    install_requires=[
        'knack==0.1.1',
        'xmljson',
        'jsbeautifier'
    ],
    extras_require={
        'test': [
        ]
    },
    entry_points={
        'console_scripts': ['bkpctl = bkpctl.entry:launch']
    }
)
