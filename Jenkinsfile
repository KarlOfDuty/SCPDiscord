pipeline
{
  agent any

  parameters
  {
    choice(name: 'BUILD_TYPE', choices: ['dev', 'pre-release', 'release'], description: 'Choose build type')
    string(name: 'RELEASE_VERSION', defaultValue: '', description: 'Enter the git tag name to create for a (pre-)release')
  }
  stages
  {
    stage('Initialize Environment')
    {
      steps
      {
        script
        {
          env.DOTNET_CLI_HOME = "/tmp/.dotnet"
          env.DEBEMAIL="xkaess22@gmail.com"
          env.DEBFULLNAME="Karl Essinger"
          env.AUR_GIT_PACKAGE="scpdiscord-git"
          env.DEV_BUILD = params.BUILD_TYPE == 'dev' ? "true" : "false"
          env.PACKAGE_NAME = params.BUILD_TYPE == 'dev' ? "scpdiscord-dev" : "scpdiscord"
          env.RPMBUILD_ARGS = params.BUILD_TYPE == 'dev' ? "--define 'dev_build true'" : ""

          common = load("${env.WORKSPACE}/ci-utilities/scripts/common.groovy")
          common.prepare_gpg_key()
        }
      }
    }
    stage('Release Pre-Checks')
    {
      when
      {
        expression { params.BUILD_TYPE != 'dev'; }
      }
      steps
      {
        script
        {
          common.verify_release_does_not_exist("KarlOfDuty/SCPDiscord", params.RELEASE_VERSION)
        }
      }
    }
    //stage('Update AUR Version')
    //{
    //  when
    //  {
    //    expression
    //    {
    //      def remoteBranch = sh(
    //        script: "curl -s 'https://aur.archlinux.org/cgit/aur.git/plain/.git_branch?h=${env.AUR_GIT_PACKAGE}'",
    //        returnStdout: true
    //      ).trim()
    //      return remoteBranch == env.BRANCH_NAME && params.BUILD_TYPE == 'dev'
    //    }
    //  }
    //  steps
    //  {
    //    script
    //    {
    //      common.update_aur_git_package(env.AUR_GIT_PACKAGE, "packaging/${env.AUR_GIT_PACKAGE}.pkgbuild", "packaging/scpdiscord.install")
    //    }
    //  }
    //}
    stage('Get Dependencies')
    {
      steps
      {
        script
        {
          if (env.BRANCH_NAME == 'beta')
          {
            sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta experimental validate +quit'
          }
          else
          {
            sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta public validate +quit'
          }
          sh 'ln -s "\$HOME/scpsl/SCPSL_Data/Managed" ".scpsl_libs"'

          dir(path: 'SCPDiscordBot')
          {
            sh 'dotnet restore'
          }
        }
      }
    }
    stage('Build Plugin')
    {
      steps
      {
        dir(path: 'SCPDiscordPlugin')
        {
          sh 'dotnet build --restore --output ./bin'
          sh 'mkdir dependencies'
          sh 'mv SCPDiscordPlugin/bin/SCPDiscord.dll ./'
          sh 'mv SCPDiscordPlugin/bin/System.Memory.dll dependencies'
          sh 'mv SCPDiscordPlugin/bin/Google.Protobuf.dll dependencies'
          sh 'mv SCPDiscordPlugin/bin/Newtonsoft.Json.dll dependencies'
          sh 'zip -r dependencies.zip dependencies'
        }
        archiveArtifacts(artifacts: 'SCPDiscordPlugin/dependencies.zip', onlyIfSuccessful: true)
        archiveArtifacts(artifacts: 'SCPDiscordPlugin/SCPDiscord.dll', onlyIfSuccessful: true)
        script
        {
          env.PLUGIN_PATH = 'SCPDiscordPlugin/SCPDiscord.dll'
          env.DEPENDENCIES_PATH = 'SCPDiscordPlugin/dependencies'
        }
        stash(includes: "${env.PLUGIN_PATH}, ${env.DEPENDENCIES_PATH}", name: "plugin-files")
      }
    }
    stage('Build Bot / Package')
    {
      parallel
      {
        stage('Basic Linux')
        {
          steps
          {
            dir(path: 'SCPDiscordBot')
            {
              sh 'dotnet publish -r linux-x64 -c Release -p:PublishTrimmed=true --self-contained true --no-restore --output linux-x64/'
              sh 'mv linux-x64/scpdiscord linux-x64/scpdiscord-sc'
              sh 'dotnet publish -r linux-x64 -c Release --self-contained false --no-restore --output linux-x64/'
            }
            archiveArtifacts(artifacts: 'SCPDiscordBot/linux-x64/scpdiscord', caseSensitive: true)
            archiveArtifacts(artifacts: 'SCPDiscordBot/linux-x64/scpdiscord-sc', caseSensitive: true)
            script
            {
              env.BASIC_LINUX_PATH = 'SCPDiscordBot/linux-x64/scpdiscord'
              env.BASIC_LINUX_SC_PATH = 'SCPDiscordBot/linux-x64/scpdiscord-sc'
            }
          }
        }
        stage('Basic Windows')
        {
          steps
          {
            sh 'dotnet publish -r win-x64 -c Release -p:PublishTrimmed=true --self-contained true --no-restore --output windows-x64/'
            sh 'mv windows-x64/scpdiscord.exe windows-x64/scpdiscord-sc.exe'
            sh 'dotnet publish -r win-x64 -c Release --self-contained false --no-restore --output windows-x64/'
            archiveArtifacts(artifacts: 'SCPDiscordBot/windows-x64/scpdiscord.exe', caseSensitive: true)
            archiveArtifacts(artifacts: 'SCPDiscordBot/windows-x64/scpdiscord-sc.exe', caseSensitive: true)
            script
            {
              env.BASIC_WINDOWS_PATH = 'SCPDiscordBot/windows-x64/scpdiscord.exe'
              env.BASIC_WINDOWS_SC_PATH = 'SCPDiscordBot/windows-x64/scpdiscord-sc.exe'
            }
          }
        }
      }
    }
  }
}
