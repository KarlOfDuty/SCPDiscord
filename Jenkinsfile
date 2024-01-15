pipeline {
    agent any

    stages {
        stage('Dependencies') {
            steps {
                sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta public-beta validate +quit'
                sh 'ln -s "\$HOME/scpsl/SCPSL_Data/Managed" ".scpsl_libs"'
                sh 'cd SCPDiscordBot; dotnet restore'
            }
        }
        stage('Build') {
            parallel {
                stage('Plugin') {
                    steps {
                        sh 'msbuild SCPDiscordPlugin/SCPDiscordPlugin.csproj -restore -p:PostBuildEvent='
                    }
                }
                stage('Bot') {
                    parallel {
                        stage('Linux') {
                            steps {
                                dir(path: 'SCPDiscordBot') {
                                    sh 'dotnet publish -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r linux-x64 -c Release --self-contained true --no-restore --output Linux-x64/'
                                }
                            }
                        }
                        stage('Windows') {
                            steps {
                                dir(path: 'SCPDiscordBot') {
                                    sh 'dotnet publish -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -r win-x64 -c Release --self-contained true --no-restore --output Windows-x64/'
                                }
                            }
                        }
                    }
                }
            }
        }
        stage('Package') {
            parallel {
                stage('Plugin') {
                    steps {
                        sh 'mkdir dependencies'
                        sh 'mv SCPDiscordPlugin/bin/SCPDiscord.dll ./'
                        sh 'mv SCPDiscordPlugin/bin/System.Memory.dll dependencies'
                        sh 'mv SCPDiscordPlugin/bin/Google.Protobuf.dll dependencies'
                        sh 'mv SCPDiscordPlugin/bin/Newtonsoft.Json.dll dependencies'
                    }
                }
                stage('Bot') {
                    steps {
                        dir(path: 'SCPDiscordBot') {
                            sh 'mv Linux-x64/SCPDiscord ../SCPDiscordBot_Linux'
                            sh 'mv Windows-x64/SCPDiscord.exe ../SCPDiscordBot_Windows.exe'
                        }
                    }
                }
            }
        }
        stage('Archive') {
            steps {
                sh 'zip -r dependencies.zip dependencies'
                archiveArtifacts(artifacts: 'dependencies.zip', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscord.dll', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Linux', onlyIfSuccessful: true)
                archiveArtifacts(artifacts: 'SCPDiscordBot_Windows.exe', onlyIfSuccessful: true)
            }
        }
    }
}
