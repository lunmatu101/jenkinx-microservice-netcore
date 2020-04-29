pipeline {
  agent {
    label "jenkins-dotnet"
  }
  environment {
    ORG = 'lunmatu101'
    APP_NAME = 'jenkinx-microservice-netcore'
    CHARTMUSEUM_CREDS = credentials('jenkins-x-chartmuseum')
    DOCKER_REGISTRY_ORG = 'lunmatu101'
  }
  stages {
    stage('CI Build and push snapshot') {
      when {
        branch 'PR-*'
      }
      environment {
        PREVIEW_VERSION = "0.0.0-SNAPSHOT-$BRANCH_NAME-$BUILD_NUMBER"
        PREVIEW_NAMESPACE = "$APP_NAME-$BRANCH_NAME".toLowerCase()
        HELM_RELEASE = "$PREVIEW_NAMESPACE".toLowerCase()
      }
      steps {
        container('dotnet') {
          sh "jx step credential -s npm-token -k file -f /builder/home/.npmrc --optional=true"
          
          dir('./src/MyLib.Tests') {
            // sh 'dotnet tool install dotnet-reportgenerator-globaltool --tool-path /tools'

            // sh 'echo "Executing TDD..."'
            // sh 'dotnet test --filter Category=TDD /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=/Coverage/cobertura.xml'
            // sh 'echo "Passed TDD"'

            // sh 'echo "Executing BDD..."'
            // sh 'dotnet test --filter Category=BDD'
            // sh 'echo "Passed BDD"'

            // sh "ls /"
            // sh '/tools/reportgenerator "-reports:/Coverage/cobertura.xml" "-targetdir:/Coverage/reports" "-reporttypes:HTML"'
          }

          dir('./src/MyLib') {
            sh 'dotnet tool install --global dotnet-sonarscanner' 
            sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet-sonarscanner begin /k:"dotnet" /d:sonar.host.url="listening-monkey-sonarqube:9000" /d:sonar.login="d66d5ec8b91f95358cfb8e9427b8a5fb81f00a64"'
            sh 'dotnet build -c Release -o ./app'
            sh 'export PATH="$PATH:$HOME/.dotnet/tools" && dotnet-sonarscanner end /d:sonar.login="d66d5ec8b91f95358cfb8e9427b8a5fb81f00a64"'
          }

          sh "export VERSION=$PREVIEW_VERSION && skaffold build -f skaffold.yaml"
          sh "jx step post build --image $DOCKER_REGISTRY/$ORG/$APP_NAME:$PREVIEW_VERSION"
          dir('./charts/preview') {
            sh "make preview"
            sh "jx preview --app $APP_NAME --dir ../.."
          }
        }
      }
    }
    stage('Build Release') {
      when {
        branch 'master'
      }
      steps {
        container('dotnet') {

          // ensure we're not on a detached head
          sh "git checkout master"
          sh "git config --global credential.helper store"
          sh "jx step git credentials"

          // so we can retrieve the version in later steps
          sh "echo \$(jx-release-version) > VERSION"
          sh "jx step tag --version \$(cat VERSION)"
          sh "jx step credential -s npm-token -k file -f /builder/home/.npmrc --optional=true"

          sh "export VERSION=`cat VERSION` && skaffold build -f skaffold.yaml"
          sh "jx step post build --image $DOCKER_REGISTRY/$ORG/$APP_NAME:\$(cat VERSION)"
        }
      }
    }
    stage('Promote to Environments') {
      when {
        branch 'master'
      }
      steps {
        container('dotnet') {
          dir('./charts/jenkinx-microservice-netcore') {
            sh "jx step changelog --batch-mode --version v\$(cat ../../VERSION)"

            // release the helm chart
            sh "jx step helm release"

            // promote through all 'Auto' promotion Environments
            sh "jx promote -b --all-auto --timeout 1h --version \$(cat ../../VERSION)"
          }
        }
      }
    }
  }
  post {
        always {
          // publishHTML target: [
          //   allowMissing: false,
          //   alwaysLinkToLastBuild: true,
          //   keepAll: true,
          //   reportDir: '/Coverage/reports',
          //   reportFiles: 'index.htm',
          //   reportName: 'Code Coverage Report'
          // ]
          cleanWs()
        }
  }
}
